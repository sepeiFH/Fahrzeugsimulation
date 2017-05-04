using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using Simulator.Simulation.Base;
using TiledSharp;
using static Simulator.Simulation.Base.Vehicle;
using Simulator.Simulation.Utilities;

namespace Simulator.Simulation
{
    public class Simulator
    {
        #region Fields
        public int RefreshesPerSecond = 50;
        public int EmergencySeconds = 10;
        private int spawningTicks;
        private int tickCountToSpawn;
        private readonly TimeSpan SimulatorSpeed;
        private readonly TimeSpan EmergencyTime;
        private Stopwatch stopWatch;
        public Stopwatch emergencyWatch;
        private List<Coordiantes> spawningPoints;
        private List<Vehicle> vehicleList;
        public List<TrafficLight> roadSignList;
        public static bool EmergencyModeActive { get; set; }
        Random rand = new Random();
        private int idCount = 0;
        public TmxMap map;
        //public List<RoadSign> allRoadSigns;
        //public List<Vehicle> allVehicles;

        public List<DynamicBlock> allDynamicObjects;

        public string mapString = File.ReadAllText("Map/FH3.tmx");

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
        public Simulator()
        {
            SimulatorSpeed = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / RefreshesPerSecond);
            EmergencyTime = TimeSpan.FromTicks(TimeSpan.TicksPerSecond * EmergencySeconds);
        }

        public Thread GetSimulatorThread()
        {
            return new Thread(TickLogic);
        }

        #endregion

        #region Init
        public void init(SimulationConfig settings)
        {
            RefreshesPerSecond = settings.Takt;
            EmergencySeconds = settings.EmergencyTime;
            spawningTicks = RefreshesPerSecond * settings.SpawnTimeFrame;
            tickCountToSpawn = spawningTicks/ settings.Fahrzeuge.Sum(x => x.SpawnRate);
            map = new TmxMap(mapString, true);
            initSpawningList();
            BlockMapping.Blocks.Count();
            allDynamicObjects = new List<DynamicBlock>();
            roadSignList = new List<TrafficLight>();
            vehicleList = new List<Vehicle>();
            EmergencyModeActive = true;
            //allRoadSigns = new List<RoadSign>();
            //allVehicles = new List<Vehicle>();

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

        public void initSpawningList()
        {
            spawningPoints = new List<Coordiantes>();
            foreach (var element in map.Layers[0].Tiles)
            {
                if (element.Gid == (int)StreetDirection.LeftToRight && element.X == 0)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 - 64, Y = element.Y * 32, Rotation = 180 });
                else if (element.Gid == (int)StreetDirection.RightToLeft && element.X == map.Width - 1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 + 96, Y = element.Y * 32 + 32, Rotation = 0 });
                else if (element.Gid == (int)StreetDirection.TopToBottom && element.Y == 0)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32 + 32, Y = element.Y * 32 - 64, Rotation = 270 });
                else if (element.Gid == (int)StreetDirection.BottomToTop && element.Y == map.Height - 1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32, Y = element.Y * 32 + 64, Rotation = 90 });
            }
        }
        #endregion

        #region Simulator Logic
        private void TickLogic()
        {
            stopWatch = new Stopwatch();
            emergencyWatch = new Stopwatch();
            stopWatch.Start();
            int tickCount=0;
            List<int> spawningList = new List<int>();
            while (true)
            {
                if (stopWatch.Elapsed > SimulatorSpeed)
                {
                    List<DynamicBlock> removeObjects = new List<DynamicBlock>();
                    if (emergencyWatch.Elapsed > EmergencyTime)
                    {
                        EmergencyModeActive = true;
                        emergencyWatch.Stop();
                    }
                    if (tickCount == 0)
                    {
                        spawningList.Clear();

                        foreach (var vehicle in Program.settings.Fahrzeuge)
                            for(int i=0; i < vehicle.SpawnRate; i++)
                                spawningList.Add(vehicle.GID);
                        tickCount++;
                    }
                    else if (tickCount++ % tickCountToSpawn == 0)
                        spawnVehicle(spawningList);

                    else if (tickCount >= spawningTicks)
                        tickCount = 0;

                    foreach (DynamicBlock dynObject in allDynamicObjects)
                    {
                        dynObject.update();
                        if (dynObject.X < -100 || dynObject.X > (map.Width + 3) * 32 || dynObject.Y < -100 || dynObject.Y > (map.Height + 3) * 32)
                            removeObjects.Add(dynObject);
                    }

                    foreach (DynamicBlock remObject in removeObjects)
                        allDynamicObjects.Remove(allDynamicObjects.Find(x => x == remObject));
                    //Update All Cars
                    /*foreach (Vehicle vehicle in allVehicles)
                        vehicle.update();

                    foreach (RoadSign roadsign in allRoadSigns)
                        roadsign.update();*/

                    //Console.WriteLine(DateTime.Now);
                    stopWatch.Restart();
                }else
                    Thread.Sleep(10);

            }
        }

        private void spawnVehicle(List<int> spawningList)
        {
            int num = rand.Next(0, spawningList.Count-1);
            setVehicle(spawningList[num]);
            spawningList.RemoveAt(num);
        }

        private void setVehicle(int tempGID)
        { 
            int randNum = rand.Next(1, spawningPoints.Count());
            Coordiantes coordinates = spawningPoints[randNum-1];
            Vehicle vehicle = new Vehicle() { GID = tempGID, Rotation = coordinates.Rotation, X = coordinates.X, Y = coordinates.Y};
            allDynamicObjects.Add(vehicle);
        }
        #endregion
    }
}
