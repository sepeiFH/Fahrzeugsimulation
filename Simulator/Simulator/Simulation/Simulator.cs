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
    /// <summary>
    /// Singleton class for the simulation logic
    /// </summary>
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
        public List<List<DynamicBlock>> dynamicObjectMap;

        //public List<RoadSign> allRoadSigns;
        //public List<Base.Vehicle> allVehicles;

        #endregion

        #region Helperclasses

        /// <summary>
        /// helper class for the simulation logic to handle X and Y coordinates and rotation in one class
        /// </summary>
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
        /// <summary>
        /// Init Method for the Simulator, in here, there are the initial instantiations of some fields and first creation of the map
        /// </summary>
        public void init()
        {
            spawningTicks = RefreshesPerSecond * Program.settings.SpawnTimeFrame;
            tickCountToSpawn = spawningTicks / Program.settings.Vehicles.Sum(x => x.SpawnRate);
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
                            roadSign.ID = ++idCount;
                            allDynamicObjects.Add(roadSign);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the external vehicle receiving. 
        /// </summary>
        /// <param name="sender">Object Sender</param>
        /// <param name="e">VehicleEventArgs</param>
        /// <returns>void</returns>
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

        /// <summary>
        /// Method to initialize the street map. Filters out all non street Block and creates a map off streetblocks and crossing blocks.
        /// Also add crossing direction to the crossing fields.
        /// </summary>
        /// <returns>void</returns>
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
        }


        /// <summary>
        /// Method to initialize the list of spawning points by checking the streetblocks which leads into the map and are on the border of the map.
        /// </summary>
        /// <returns>void</returns>
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

        /// <summary>
        /// Method contains the main loop of the simulation in which the simulation logic includig the call of the block updates run.
        /// </summary>
        /// <returns>void</returns>
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
                        //Thread updateThread = new Thread(dynObject.update);
                        //updateThread.Start();
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
                //else
                //Thread.Sleep(10);

            }
        }

        /// <summary>
        /// Method to choose a vehicle out of the vehicles to spawn and remove the spawned vehicle from the spawning list.
        /// </summary>
        /// <param name="spawningList">List of integer which contains the GID of the vehicles to spawn</param>
        /// <returns>void</returns>
        private void spawnVehicle(List<int> spawningList)
        {
            int num = rand.Next(0, spawningList.Count - 1);
            setVehicle(spawningList.ElementAt(num));
            spawningList.RemoveAt(num);
        }

        /// <summary>
        /// Method to create a new vehicle at a random spawning point and add the vehicle to the dynamicObject List.
        /// </summary>
        /// <param name="tempGID">GID of the vehicle to spawn</param>
        /// <returns>void</returns>
        private void setVehicle(int tempGID)
        {
            int randNum = rand.Next(1, spawningPoints.Count());
            Coordiantes coordinates = spawningPoints[randNum - 1];
            ConfigVehicle tempVehicle = Program.settings.Vehicles.Find(x => x.GID == tempGID);
            Base.Vehicle vehicle = new Base.Vehicle(rand, tempGID, tempVehicle.MaxVelocity, tempVehicle.MaxAcceleration, tempVehicle.MaxDeceleration) { Rotation = coordinates.Rotation, X = coordinates.X, Y = coordinates.Y, Length = tempVehicle.Length, Width = tempVehicle.Width, vehicleType = tempVehicle.Type.Contains("KFZ1") ? VehicleList.Car1 : (tempVehicle.Type.Contains("KFZ2") ? VehicleList.Car2 : (tempVehicle.Type.Contains("LKW1") ? VehicleList.Truck1 : (tempVehicle.Type.Contains("LKW2") ? VehicleList.Truck2 : VehicleList.Motorcycle))) };
            allDynamicObjects.Add(vehicle);
        }
        #endregion

        #region vehicle interaction

        /*
        // second try: Get a cutted view of the overall streetmap
        public List<List<StreetBlock>> getMapInfo2(double vehicleX, double vehicleY, double rotation, Dictionary<VehicleMovementAgent.side, int> directionDistances, ref int actPosInMapListX, ref int actPosInMapListY)
        {
            List<List<StreetBlock>> vehiclesMap = new List<List<StreetBlock>>();

            return vehiclesMap;
        }

        // first try: Get a new created map out of the tile-map
        */

        /// <summary>
        /// Method to create the map in the sight of the vehicle in the vehicle-given view-range
        /// </summary>
        /// <param name="vehicleX">actual X coordinates of the vehicle</param>
        /// <param name="vehicleY">actual Y coordinates of the vehicle</param>
        /// <param name="rotation">actual rotation of the vehicle</param>
        /// <param name="directionDistances">Dictionary which contains the distances for the viewing range</param>
        /// <param name="actPosInMapListX">Reference to the actual x position in the generated map</param>
        /// <param name="actPosInMapListY">Reference to the actual y position in the generated map</param>
        /// <param name="actoffsetX">Reference to the difference between the x coordinates and the x block Position*32</param>
        /// <param name="actoffsetY">Reference to the difference between the y coordinates and the y block Position*32</param>
        /// <returns>List<List<StreetBlock>></returns>
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
                        actPosInMapListY = vehiclesMap.Count() - 1;
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

                //vehiclesMap.Reverse();
                //foreach (var row in vehiclesMap)
                //    row.Reverse();
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
                        actPosInMapListX = i % vehiclesMap.Count - 1;
                        actPosInMapListY = vehiclesMap[i % vehiclesMap.Count].Count - 1;
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
                        actPosInMapListX = colums.Count() - 1;
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

        /// <summary>
        /// Method to add a street or crossing block of the given GID (param 1) to the given position (param 2 and 3)
        /// </summary>
        /// <param name="GID">GID of the block to set/param>
        /// <param name="posX">x position to set the block</param>
        /// <param name="posY">y position to set the block</param>
        /// <returns>StreetBlock</returns>
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

        /// <summary>
        /// TODO: Method to get the dynamic blocks
        /// </summary>
        /// <param name="vehicleX">actual X coordinates of the vehicle</param>
        /// <param name="vehicleY">actual Y coordinates of the vehicle</param>
        /// <param name="rotation">actual rotation of the vehicle</param>
        /// <param name="directionDistances">Dictionary which contains the distances for the viewing range</param>
        /// <returns>StreetBlock</returns>
        public SortedDictionary<double, List<DynamicBlock>> allDynamicObjectsInRange(double vehicleX, double vehicleY, double rotation, Dictionary<VehicleMovementAgent.side, int> directionDistances)
        {
            SortedDictionary<double, List<DynamicBlock>> dynamicBlocksWithDistance = new SortedDictionary<double, List<DynamicBlock>>();
            // around 0 degree
            if (rotation >= 0 && rotation < 45 || rotation >= 315 && rotation < 360)
            {
                double backwardBorder = vehicleX + directionDistances[VehicleMovementAgent.side.backward] * 32;
                double forewardBorder = vehicleX - directionDistances[VehicleMovementAgent.side.foreward] * 32;
                double leftBorder = vehicleY + directionDistances[VehicleMovementAgent.side.left] * 32;
                double rightBorder = vehicleY - directionDistances[VehicleMovementAgent.side.right] * 32;

                //vehiclesDynamicBlocks.AddRange(
                foreach (var block in allDynamicObjects.FindAll(x => (x.X > forewardBorder && x.X < backwardBorder) &&
                                                                         (x.Y > rightBorder && x.Y < leftBorder) &&
                                                                         (x.X != vehicleX && x.Y != vehicleY)))
                {
                    double distance = Math.Sqrt(Math.Pow(vehicleX - block.X, 2) + Math.Pow(vehicleY - block.Y, 2));
                    if (!dynamicBlocksWithDistance.ContainsKey(distance))
                    {
                        List<DynamicBlock> tempList = new List<DynamicBlock>();
                        tempList.Add(block);
                        dynamicBlocksWithDistance.Add(distance, tempList);
                    }
                    else
                    {
                        List<DynamicBlock> templist;
                        dynamicBlocksWithDistance.TryGetValue(distance, out templist);
                        templist.Add(block);
                    }
                }
            }
            // around 90 degree
            else if (rotation >= 45 && rotation < 135)
            {
                double backwardBorder = vehicleY + directionDistances[VehicleMovementAgent.side.backward] * 32;
                double forewardBorder = vehicleY - directionDistances[VehicleMovementAgent.side.foreward] * 32;
                double leftBorder = vehicleX - directionDistances[VehicleMovementAgent.side.left] * 32;
                double rightBorder = vehicleX + directionDistances[VehicleMovementAgent.side.right] * 32;
                foreach (var block in allDynamicObjects.FindAll(x => (x.X >= leftBorder && x.X <= rightBorder) &&
                                                                        (x.Y >= forewardBorder && x.Y <= backwardBorder) &&
                                                                        (x.X != vehicleX && x.Y != vehicleY)))
                {
                    double distance = Math.Sqrt(Math.Pow(vehicleX - block.X, 2) + Math.Pow(vehicleY - block.Y, 2));
                    if (!dynamicBlocksWithDistance.ContainsKey(distance))
                    {
                        List<DynamicBlock> tempList = new List<DynamicBlock>();
                        tempList.Add(block);
                        dynamicBlocksWithDistance.Add(distance, tempList);
                    }
                    else
                    {
                        List<DynamicBlock> templist;
                        dynamicBlocksWithDistance.TryGetValue(distance, out templist);
                        templist.Add(block);
                    }
                }
            }
            // around 180 degree
            else if (rotation >= 135 && rotation < 225)
            {
                double backwardBorder = vehicleX - directionDistances[VehicleMovementAgent.side.backward] * 32;
                double forewardBorder = vehicleX + directionDistances[VehicleMovementAgent.side.foreward] * 32;
                double leftBorder = vehicleY - directionDistances[VehicleMovementAgent.side.left] * 32;
                double rightBorder = vehicleY + directionDistances[VehicleMovementAgent.side.right] * 32;
                foreach (var block in allDynamicObjects.FindAll(x => (x.X >= backwardBorder && x.X <= forewardBorder) &&
                                                                        (x.Y >= leftBorder && x.Y <= rightBorder) &&
                                                                        (x.X != vehicleX && x.Y != vehicleY)))
                {
                    double distance = Math.Sqrt(Math.Pow(vehicleX - block.X, 2) + Math.Pow(vehicleY - block.Y, 2));
                    if (!dynamicBlocksWithDistance.ContainsKey(distance))
                    {
                        List<DynamicBlock> tempList = new List<DynamicBlock>();
                        tempList.Add(block);
                        dynamicBlocksWithDistance.Add(distance, tempList);
                    }
                    else
                    {
                        List<DynamicBlock> templist;
                        dynamicBlocksWithDistance.TryGetValue(distance, out templist);
                        templist.Add(block);
                    }
                }
            }
            // around 270 degree
            else
            {
                double backwardBorder = vehicleY - directionDistances[VehicleMovementAgent.side.backward] * 32;
                double forewardBorder = vehicleY + directionDistances[VehicleMovementAgent.side.foreward] * 32;
                double leftBorder = vehicleX + directionDistances[VehicleMovementAgent.side.left] * 32;
                double rightBorder = vehicleX - directionDistances[VehicleMovementAgent.side.right] * 32;


                foreach (var block in allDynamicObjects.FindAll(x => (x.X >= rightBorder && x.X <= leftBorder) &&
                                                                        (x.Y >= backwardBorder && x.Y <= forewardBorder) &&
                                                                        (x.X != vehicleX && x.Y != vehicleY)))
                {
                    double distance = Math.Sqrt(Math.Pow(vehicleX - block.X, 2) + Math.Pow(vehicleY - block.Y, 2));
                    if (!dynamicBlocksWithDistance.ContainsKey(distance))
                    {
                        List<DynamicBlock> tempList = new List<DynamicBlock>();
                        tempList.Add(block);
                        dynamicBlocksWithDistance.Add(distance, tempList);
                    }
                    else
                    {
                        List<DynamicBlock> templist;
                        dynamicBlocksWithDistance.TryGetValue(distance, out templist);
                        templist.Add(block);
                    }
                }
            }

            return dynamicBlocksWithDistance;
        }

        #endregion


    }
}
