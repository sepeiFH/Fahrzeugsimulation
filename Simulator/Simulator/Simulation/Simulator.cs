using Simulator.Simulation.Base;
using Simulator.Simulation.Utilities;
using Simulator.VehicleAgents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using TiledSharp;
using VehicleHandoverLibrary;
using static Simulator.Simulation.Base.Vehicle;

namespace Simulator.Simulation
{
    public sealed class Simulator
    {
        #region Singleton
        private static readonly Simulator instance = new Simulator();

        private Simulator()
        {
            SimulatorSpeed = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / RefreshesPerSecond);
            EmergencyTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * EmergencySeconds);
        }

        public static Simulator Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion
        #region Fields
        private int spawningTicks;
        private int tickCountToSpawn;
        private Stopwatch stopWatch;
        private List<Base.Vehicle> vehicleList;
        private List<CrossingBlock> crossingBlocks;
        private List<List<StreetBlock>> Streetmap;
        private readonly TimeSpan SimulatorSpeed;
        private readonly TimeSpan EmergencyTime;
        private VehicleReceiver vehicleReceiver;
        private VehicleSender vehicleSender;
        private static int idCount = 0;
        private static Random rand = new Random();
        private static List<Coordiantes> spawningPoints;

        public static bool EmergencyModeActive { get; set; }
        public int RefreshesPerSecond = Program.settings.Takt;
        public int EmergencySeconds = Program.settings.EmergencyTime;
        public double maxTurningSpeed;
        public Stopwatch emergencyWatch;
        public TmxMap map;
        public string mapString = File.ReadAllText("Map/FH3.tmx");
        public static List<Base.Vehicle> vehiclesToAdd = new List<Base.Vehicle>();
        public List<DynamicBlock> allDynamicObjects;
        public static List<Base.Vehicle> receivingVehicles;
        public List<TrafficLight> roadSignList;

        //public List<RoadSign> allRoadSigns;
        //public List<Base.Vehicle> allVehicles;

        #endregion

        #region Helperclasses
        private class Coordiantes
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Rotation { get; set; }
        }

        #endregion

        #region Constructor

        public Thread GetSimulatorThread()
        {
            return new Thread(TickLogic);
        }

        #endregion

        #region Init
        public void init(SimulationConfig settings)
        {
            //RefreshesPerSecond = settings.Takt;
            //EmergencySeconds = settings.EmergencyTime;
            spawningTicks = RefreshesPerSecond * settings.SpawnTimeFrame;
            tickCountToSpawn = spawningTicks / settings.Vehicles.Sum(x => x.SpawnRate);
            map = new TmxMap(mapString, true);
            initStreetMap();
            initSpawningList();
            BlockMapping.Blocks.Count();
            allDynamicObjects = new List<DynamicBlock>();
            roadSignList = new List<TrafficLight>();
            vehicleList = new List<Base.Vehicle>();
            receivingVehicles = new List<Base.Vehicle>();
            EmergencyModeActive = true;
            vehicleReceiver = new VehicleReceiver(Groups.GROUP01);
            vehicleReceiver.ReceiveEventHandler += VehicleReceiver_ReceiveEventHandler;
            vehicleSender = new VehicleSender(Groups.GROUP01);
            maxTurningSpeed = (Program.settings.TurningSpeed * 10) / Program.settings.Takt;
            //allRoadSigns = new List<RoadSign>();
            //allVehicles = new List<Base.Vehicle>();

            if (map.ObjectGroups.Count > 0)
            {
                foreach (var group in map.ObjectGroups)
                {
                    if (group.Name == "Verkehrszeichen")
                    {
                        foreach (TmxObject obj in group.Objects)
                        {
                            RoadSign roadSign = new RoadSign() { Block = obj };
                            if (obj.Tile.Gid == TrafficLight.RedGID)
                            {
                                roadSign = new TrafficLight() { Block = obj };
                                roadSignList.Add((TrafficLight)roadSign);
                            }
                            //allRoadSigns.Add(roadSign);
                            roadSign.ID = ++idCount;
                            allDynamicObjects.Add(roadSign);
                            //if (roadSign.GID == (int)TrafficLight.LightStatus.Red)
                        }
                    }
                }
            }
        }

        private static void VehicleReceiver_ReceiveEventHandler(object sender, VehicleEventArgs e)
        {
            var zeroSpawningPoints = spawningPoints.FindAll(x => x.X <= 0);
            var spawningPoint = zeroSpawningPoints.ElementAt(rand.Next(zeroSpawningPoints.Count - 1));
            List<ConfigVehicle> vehicleList;
            ConfigVehicle randVehicle;

            if (e.Vehicle.Type.Equals(VehicleType.CAR))
            {
                vehicleList = Program.settings.Vehicles.FindAll(x => x.Type.Contains("KFZ"));
                randVehicle = vehicleList.ElementAt(rand.Next(vehicleList.Count));
            }
            else if (e.Vehicle.Type.Equals(VehicleType.TRUCK))
            {
                vehicleList = Program.settings.Vehicles.FindAll(x => x.Type.Contains("LKW"));
                randVehicle = vehicleList.ElementAt(rand.Next(vehicleList.Count));
            }
            else
            {
                vehicleList = Program.settings.Vehicles.FindAll(x => x.Type.Contains("MFZ"));
                randVehicle = vehicleList.ElementAt(rand.Next(vehicleList.Count));
            }

            Base.Vehicle newVehicle = new Base.Vehicle(rand, randVehicle.GID, e.Vehicle.MaxVelocity, e.Vehicle.MaxAcceleration, e.Vehicle.MaxDeceleration);

            newVehicle.ID = ++idCount;
            newVehicle.Length = e.Vehicle.Length * 6;
            newVehicle.Width = e.Vehicle.Width * 6;
            newVehicle.X = spawningPoint.X;
            newVehicle.Y = spawningPoint.Y;
            newVehicle.Rotation = 180;
            newVehicle.vehicleType = randVehicle.Type.Equals("KFZ1") ? VehicleList.Car1 : (randVehicle.Type.Equals("KFZ2") ? VehicleList.Car2 : (randVehicle.Type.Equals("LKW1") ? VehicleList.Truck1 : (randVehicle.Type.Equals("LKW2") ? VehicleList.Truck2 : VehicleList.Motorcycle)));

            receivingVehicles.Add(newVehicle);
        }

        private void initStreetMap()
        {
            Streetmap = new List<List<StreetBlock>>();
            crossingBlocks = new List<CrossingBlock>();
            List<StreetBlock> colums = new List<StreetBlock>();
            foreach (var tile in map.Layers.First().Tiles.ToList())
            {
                colums.Add(addStreetBlock(tile.Gid, tile.X, tile.Y));
                if (tile.X == map.Width - 1)
                {
                    Streetmap.Add(colums);
                    colums = new List<StreetBlock>();
                }
            }
            for (int i = 0; i < Streetmap.Count; i++)
                for (int j = 0; j < Streetmap[i].Count; j++)
                {
                    if (Streetmap[i][j] != null)
                        switch (Streetmap[i][j].Direction)
                        {
                            case StreetDirection.BottomToTop:
                                if (i - 1 >= 0 && Streetmap[i - 1][j] != null && Streetmap[i - 1][j].Direction == StreetDirection.Crossing)
                                {
                                    List<CrossingDirection> crossingDirs = new List<CrossingDirection>();
                                    if (i - 3 >= 0 && Streetmap[i - 3][j] != null && Streetmap[i - 3][j].Direction == StreetDirection.BottomToTop)
                                        crossingDirs.Add(CrossingDirection.Straight);
                                    if (i - 2 >= 0 && j - 2 >= 0 && Streetmap[i - 2][j - 2] != null && Streetmap[i - 2][j - 2].Direction == StreetDirection.RightToLeft)
                                        crossingDirs.Add(CrossingDirection.Left);
                                    if (i - 1 >= 0 && j + 1 >= 0 && Streetmap[i - 1][j + 1] != null && Streetmap[i - 1][j + 1].Direction == StreetDirection.LeftToRight)
                                        crossingDirs.Add(CrossingDirection.Right);

                                    Streetmap[i][j] = new CrossingBlock() { GID = Streetmap[i][j].GID, Direction = Streetmap[i][j].Direction, PossibleCrosDirs = crossingDirs, posX = j, posY = i };
                                    crossingBlocks.Add((CrossingBlock)Streetmap[i][j]);
                                }
                                break;
                            case StreetDirection.Crossing:
                                break;
                            case StreetDirection.LeftToRight:
                                if (j + 1 < Streetmap[i].Count && Streetmap[i][j + 1] != null && Streetmap[i][j + 1].Direction == StreetDirection.Crossing)
                                {
                                    List<CrossingDirection> crossingDirs = new List<CrossingDirection>();
                                    if (j + 3 >= 0 && Streetmap[i][j + 3] != null && Streetmap[i][j + 3].Direction == StreetDirection.LeftToRight)
                                        crossingDirs.Add(CrossingDirection.Straight);
                                    if (i - 2 >= 0 && j + 2 >= 0 && Streetmap[i - 2][j + 2] != null && Streetmap[i - 2][j + 2].Direction == StreetDirection.BottomToTop)
                                        crossingDirs.Add(CrossingDirection.Left);
                                    if (i + 1 >= 0 && j + 1 >= 0 && Streetmap[i + 1][j + 1] != null && Streetmap[i + 1][j + 1].Direction == StreetDirection.TopToBottom)
                                        crossingDirs.Add(CrossingDirection.Right);

                                    Streetmap[i][j] = new CrossingBlock() { GID = Streetmap[i][j].GID, Direction = Streetmap[i][j].Direction, PossibleCrosDirs = crossingDirs, posX = j, posY = i };
                                    crossingBlocks.Add((CrossingBlock)Streetmap[i][j]);
                                }
                                break;
                            case StreetDirection.RightToLeft:
                                if (j - 1 >= 0 && Streetmap[i][j - 1] != null && Streetmap[i][j - 1].Direction == StreetDirection.Crossing)
                                {
                                    List<CrossingDirection> crossingDirs = new List<CrossingDirection>();
                                    if (j - 3 >= 0 && Streetmap[i][j - 3] != null && Streetmap[i][j - 3].Direction == StreetDirection.RightToLeft)
                                        crossingDirs.Add(CrossingDirection.Straight);
                                    if (i + 2 >= 0 && j - 2 >= 0 && Streetmap[i + 2][j - 2] != null && Streetmap[i + 2][j - 2].Direction == StreetDirection.TopToBottom)
                                        crossingDirs.Add(CrossingDirection.Left);
                                    if (i - 1 >= 0 && j - 1 >= 0 && Streetmap[i - 1][j - 1] != null && Streetmap[i - 1][j - 1].Direction == StreetDirection.BottomToTop)
                                        crossingDirs.Add(CrossingDirection.Right);

                                    Streetmap[i][j] = new CrossingBlock() { GID = Streetmap[i][j].GID, Direction = Streetmap[i][j].Direction, PossibleCrosDirs = crossingDirs, posX = j, posY = i };
                                    crossingBlocks.Add((CrossingBlock)Streetmap[i][j]);
                                }
                                break;
                            case StreetDirection.TopToBottom:
                                if (i + 1 < Streetmap.Count && Streetmap[i + 1][j] != null && Streetmap[i + 1][j].Direction == StreetDirection.Crossing)
                                {
                                    List<CrossingDirection> crossingDirs = new List<CrossingDirection>();
                                    if (i + 3 >= 0 && Streetmap[i + 3][j] != null && Streetmap[i + 3][j].Direction == StreetDirection.TopToBottom)
                                        crossingDirs.Add(CrossingDirection.Straight);
                                    if (i + 2 >= 0 && j + 2 >= 0 && Streetmap[i + 2][j + 2] != null && Streetmap[i + 2][j + 2].Direction == StreetDirection.LeftToRight)
                                        crossingDirs.Add(CrossingDirection.Left);
                                    if (i + 1 >= 0 && j - 1 >= 0 && Streetmap[i + 1][j - 1] != null && Streetmap[i + 1][j - 1].Direction == StreetDirection.RightToLeft)
                                        crossingDirs.Add(CrossingDirection.Right);

                                    Streetmap[i][j] = new CrossingBlock() { GID = Streetmap[i][j].GID, Direction = Streetmap[i][j].Direction, PossibleCrosDirs = crossingDirs, posX = j, posY = i };
                                    crossingBlocks.Add((CrossingBlock)Streetmap[i][j]);
                                }
                                break;
                        }
                }


            // DebugPart
            Console.Clear();
            foreach (List<StreetBlock> row in Streetmap)
            {
                foreach (StreetBlock entry in row)
                {
                    if (entry == null)
                        Console.Write("0");
                    else
                        Console.Write(entry.GID);
                }
                Console.Write("\n");
            }
            int bla = 0;
        }

        public void initSpawningList()
        {
            spawningPoints = new List<Coordiantes>();
            foreach (var element in map.Layers[0].Tiles)
            {
                if (element.Gid == (int)StreetDirection.LeftToRight && element.X == 0)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 - 64, Y = element.Y * 32 + 16, Rotation = 180 });
                else if (element.Gid == (int)StreetDirection.RightToLeft && element.X == map.Width - 1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 + 96, Y = element.Y * 32 + 16, Rotation = 0 });
                else if (element.Gid == (int)StreetDirection.TopToBottom && element.Y == 0)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 + 16, Y = element.Y * 32 - 64, Rotation = 270 });
                else if (element.Gid == (int)StreetDirection.BottomToTop && element.Y == map.Height - 1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 + 16, Y = element.Y * 32 + 64, Rotation = 90 });
            }
        }
        #endregion

        #region Simulator Logic
        private void TickLogic()
        {
            stopWatch = new Stopwatch();
            emergencyWatch = new Stopwatch();
            stopWatch.Start();
            int tickCount = 0;
            List<int> spawningList = new List<int>();
            while (true)
            {
                if (stopWatch.Elapsed > SimulatorSpeed)
                {
                    foreach (var addVehicle in vehiclesToAdd)
                        allDynamicObjects.Add(addVehicle);
                    vehiclesToAdd.Clear();
                    List<DynamicBlock> removeObjects = new List<DynamicBlock>();
                    if (emergencyWatch.Elapsed > EmergencyTime)
                    {
                        EmergencyModeActive = true;
                        emergencyWatch.Stop();
                    }
                    if (tickCount == 0)
                    {
                        spawningList.Clear();

                        foreach (var vehicle in Program.settings.Vehicles)
                            for (int i = 0; i < vehicle.SpawnRate; i++)
                                spawningList.Add(vehicle.GID);
                        tickCount++;
                    }
                    else if (tickCount++ % tickCountToSpawn == 0)
                        spawnVehicle(spawningList);

                    else if (tickCount >= spawningTicks)
                        tickCount = 0;

                    if (receivingVehicles.Count > 0)
                    {
                        allDynamicObjects.AddRange(receivingVehicles);
                        receivingVehicles.Clear();
                    }

                    foreach (DynamicBlock dynObject in allDynamicObjects)
                    {
                        dynObject.update();
                        if (dynObject.X > (map.Width + 3) * 32 && dynObject.GetType().Equals(typeof(Base.Vehicle)))
                        {
                            var actVehicle = (Base.Vehicle)dynObject;
                            var vehicle = new VehicleHandoverLibrary.Vehicle();
                            vehicle.Length = actVehicle.Length / 6;
                            vehicle.Width = actVehicle.Width / 6;
                            vehicle.MaxAcceleration = (actVehicle.driver.MaxAcceleration * Program.settings.Takt) / 10;
                            vehicle.MaxDeceleration = (actVehicle.driver.MaxDeceleration * Program.settings.Takt) / 10;
                            vehicle.MaxVelocity = (actVehicle.driver.MaxVelocity * Program.settings.Takt) / 10;
                            if (actVehicle.vehicleType == VehicleList.Car1 || actVehicle.vehicleType == VehicleList.Car2)
                                vehicle.Type = VehicleType.CAR;
                            else if (actVehicle.vehicleType == VehicleList.Truck1 || actVehicle.vehicleType == VehicleList.Truck2)
                                vehicle.Type = VehicleType.TRUCK;
                            else
                                vehicle.Type = VehicleType.BIKE;
                            // Push vehicle
                            vehicleSender.PushVehicle(vehicle);
                        }

                        if (dynObject.X < -100 || dynObject.X > (map.Width + 3) * 32 || dynObject.Y < -100 || dynObject.Y > (map.Height + 3) * 32)
                            removeObjects.Add(dynObject);
                    }

                    foreach (DynamicBlock remObject in removeObjects)
                    {
                        allDynamicObjects.Remove(remObject);
                    }
                    stopWatch.Restart();
                }
                else
                    Thread.Sleep(10);

            }
        }

        private void spawnVehicle(List<int> spawningList)
        {
            int num = rand.Next(1, spawningList.Count);
            //setVehicle(spawningList[num - 1], num - 1);
            //spawningList.RemoveAt(num - 1);
        }

        private void setVehicle(int tempGID, int listPlace)
        {
            int randNum = rand.Next(1, spawningPoints.Count());
            Coordiantes coordinates = spawningPoints[randNum - 1];
            ConfigVehicle tempVehicle = Program.settings.Vehicles.Find(x => x.GID == tempGID);
            Base.Vehicle vehicle = new Base.Vehicle(rand, tempGID, tempVehicle.MaxVelocity, tempVehicle.MaxAcceleration, tempVehicle.MaxDeceleration) { Rotation = coordinates.Rotation, X = coordinates.X, Y = coordinates.Y, Length = tempVehicle.Length, Width = tempVehicle.Width, vehicleType = tempVehicle.Type.Contains("KFZ1") ? VehicleList.Car1 : (tempVehicle.Type.Contains("KFZ2") ? VehicleList.Car2 : (tempVehicle.Type.Contains("LKW1") ? VehicleList.Truck1 : (tempVehicle.Type.Contains("LKW2") ? VehicleList.Truck2 : VehicleList.Motorcycle))) };
            allDynamicObjects.Add(vehicle);
        }
        #endregion

        #region vehicle interaction

        // second try: Get a cutted view of the overall streetmap
        public List<List<StreetBlock>> getMapInfo2(double vehicleX, double vehicleY, double rotation, Dictionary<VehicleMovementAgent.side, int> directionDistances, ref int actPosInMapListX, ref int actPosInMapListY)
        {
            List<List<StreetBlock>> vehiclesMap = new List<List<StreetBlock>>();

            return vehiclesMap;
        }

        // first try: Get a new created map out of the tile-map
        public List<List<StreetBlock>> getMapInfo(double vehicleX, double vehicleY, double rotation, Dictionary<VehicleMovementAgent.side, int> directionDistances, ref int actPosInMapListX, ref int actPosInMapListY, ref int actoffsetX, ref int actoffsetY)
        {
            List<List<StreetBlock>> vehiclesMap = new List<List<StreetBlock>>();
            // find block in which the vehicle is

            int xBlock = (int)Math.Round(vehicleX / (double)map.TileWidth);
            int yBlock = (int)Math.Round(vehicleY / (double)map.TileHeight) - 1;
            actoffsetX = (int)vehicleX % 32;
            actoffsetY = (int)vehicleY % 32;

            List<TmxLayerTile> tiles = new List<TmxLayerTile>();

            // around 0 degree
            if (rotation >= 0 && rotation < 45 || rotation >= 315 && rotation < 360)
            {
                yBlock -= 1;
                int backwardBorder = xBlock + directionDistances[VehicleMovementAgent.side.backward];
                int forewardBorder = xBlock - directionDistances[VehicleMovementAgent.side.foreward];
                int leftBorder = yBlock + directionDistances[VehicleMovementAgent.side.left];
                int rightBorder = yBlock - directionDistances[VehicleMovementAgent.side.right];
                tiles = map.Layers.First().Tiles.ToList().FindAll(x => (x.X > forewardBorder - 1 && x.X < backwardBorder + 1) &&
                                                                        (x.Y > rightBorder - 1 && x.Y < leftBorder + 1));
                List<StreetBlock> colums = new List<StreetBlock>();
                tiles.Reverse();
                foreach (var tile in tiles)
                {
                    if (tile.X == xBlock && tile.Y == yBlock)
                    {
                        actPosInMapListY = vehiclesMap.Count();
                        actPosInMapListX = colums.Count();
                    }
                    colums.Add(addStreetBlock(tile.Gid, tile.X, tile.Y));
                    if ((tile.X == forewardBorder || tile.X == 0) && !tile.Equals(tiles[0]))
                    {
                        vehiclesMap.Add(colums);
                        colums = new List<StreetBlock>();
                    }
                }

                vehiclesMap.Add(colums);
            }
            // around 90 degree
            else if (rotation >= 45 && rotation < 135)
            {
                int backwardBorder = yBlock + directionDistances[VehicleMovementAgent.side.backward];
                int forewardBorder = yBlock - directionDistances[VehicleMovementAgent.side.foreward];
                int leftBorder = xBlock - directionDistances[VehicleMovementAgent.side.left];
                int rightBorder = xBlock + directionDistances[VehicleMovementAgent.side.right];
                tiles = map.Layers.First().Tiles.ToList().FindAll(x => (x.X >= leftBorder && x.X <= rightBorder) &&
                                                                        (x.Y >= forewardBorder && x.Y <= backwardBorder));


                List<StreetBlock> colums = new List<StreetBlock>();
                foreach (var tile in tiles)
                {
                    if ((tile.X == leftBorder || tile.X == 0) && !tile.Equals(tiles[0]))
                    {
                        break;
                    }
                    else
                    {
                        vehiclesMap.Add(new List<StreetBlock>());
                    }
                }

                for (int i = tiles.Count - 1; i >= 0; i--)
                {
                    if (tiles[i].X == xBlock && tiles[i].Y == yBlock)
                    {
                        actPosInMapListX = i % vehiclesMap.Count;
                        actPosInMapListY = vehiclesMap[i % vehiclesMap.Count].Count;
                    }
                    vehiclesMap[i % vehiclesMap.Count].Add(addStreetBlock(tiles[i].Gid, tiles[i].X, tiles[i].Y));
                }

            }
            // around 180 degree
            else if (rotation >= 135 && rotation < 225)
            {
                int backwardBorder = xBlock - directionDistances[VehicleMovementAgent.side.backward];
                int forewardBorder = xBlock + directionDistances[VehicleMovementAgent.side.foreward];
                int leftBorder = yBlock - directionDistances[VehicleMovementAgent.side.left];
                int rightBorder = yBlock + directionDistances[VehicleMovementAgent.side.right];
                tiles = map.Layers.First().Tiles.ToList().FindAll(x => (x.X >= backwardBorder && x.X <= forewardBorder) &&
                                                                        (x.Y >= leftBorder && x.Y <= rightBorder));
                List<StreetBlock> colums = new List<StreetBlock>();

                foreach (var tile in tiles)
                {
                    if ((tile.X == backwardBorder || tile.X == 0) && !tile.Equals(tiles[0]))
                    {
                        vehiclesMap.Add(colums);
                        colums = new List<StreetBlock>();
                    }
                    if (tile.X == xBlock && tile.Y == yBlock)
                    {
                        actPosInMapListY = vehiclesMap.Count() + 1;
                        actPosInMapListX = colums.Count() - 2;
                    }
                    colums.Add(addStreetBlock(tile.Gid, tile.X, tile.Y));
                }

                vehiclesMap.Add(colums);
            }
            // around 270 degree
            else
            {
                int backwardBorder = yBlock - directionDistances[VehicleMovementAgent.side.backward];
                int forewardBorder = yBlock + directionDistances[VehicleMovementAgent.side.foreward];
                int leftBorder = xBlock + directionDistances[VehicleMovementAgent.side.left];
                int rightBorder = xBlock - directionDistances[VehicleMovementAgent.side.right];
                tiles = map.Layers.First().Tiles.ToList().FindAll(x => (x.X >= rightBorder && x.X <= leftBorder) &&
                                                                        (x.Y >= backwardBorder && x.Y <= forewardBorder));
                List<StreetBlock> colums = new List<StreetBlock>();
                foreach (var tile in tiles)
                    if ((tile.X == rightBorder || tile.X == 0) && !tile.Equals(tiles[0]))
                        break;
                    else
                        vehiclesMap.Add(new List<StreetBlock>());


                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i].X == xBlock && tiles[i].Y == yBlock)
                    {
                        actPosInMapListX = i % vehiclesMap.Count;
                        actPosInMapListY = vehiclesMap[i % vehiclesMap.Count].Count;
                    }
                    vehiclesMap[i % vehiclesMap.Count].Add(addStreetBlock(tiles[i].Gid, tiles[i].X, tiles[i].Y));
                }
            }

            return vehiclesMap;
        }

        private StreetBlock addStreetBlock(int GID, int posX, int posY)
        {
            StreetBlock block = null;
            CrossingBlock crossingBlock = crossingBlocks.Find(x => x.posX == posX && x.posY == posY);
            if (crossingBlock != null)
                block = crossingBlock;
            else
                switch (GID)
                {
                    case 0:
                        block = new StreetWithVehicle();
                        break;
                    case (int)StreetDirection.BottomToTop:
                        block = new BlockStreetBottomToTop();
                        break;
                    case (int)StreetDirection.LeftToRight:
                        block = new BlockStreetLeftToRight();
                        break;
                    case (int)StreetDirection.RightToLeft:
                        block = new BlockStreetRightToLeft();
                        break;
                    case (int)StreetDirection.TopToBottom:
                        block = new BlockStreetTopToDown();
                        break;
                    case (int)StreetDirection.Crossing:
                        block = new CrossingElement1();
                        break;
                    default:
                        block = null;
                        break;
                }
            return block;
        }

        public List<List<DynamicBlock>> getDynamicInfo(double vehicleX, double vehicleY, Dictionary<VehicleMovementAgent.side, int> directionDistances)
        {
            List<List<DynamicBlock>> vehiclesDynamicBlocks = new List<List<DynamicBlock>>();
            // find block in which the vehicle is
            int xBlock = (int)vehicleX / map.TileWidth;
            int yBlock = (int)vehicleY / map.TileHeight;
            foreach (var side in directionDistances)
            {
                List<TmxLayerTile> tiles = map.Layers.First().Tiles.ToList().FindAll(x => x.X == xBlock);

            }

            return vehiclesDynamicBlocks;
        }
        #endregion


    }
}
