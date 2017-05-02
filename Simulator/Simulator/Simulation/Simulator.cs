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

namespace Simulator.Simulation
{
    public class Simulator
    {
        #region Fields
        public readonly int RefreshesPerSecond = 50;
        private readonly TimeSpan SimulatorSpeed;
        private Stopwatch stopWatch;
        private List<Coordiantes> spawningPoints;
        Random rand = new Random();
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
        }

        public Thread GetSimulatorThread()
        {
            return new Thread(TickLogic);
        }

        #endregion


        #region Simulator Logic
        private void TickLogic()
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true)
            {
                if (stopWatch.Elapsed > SimulatorSpeed)
                {
                    spawnVehicle();
                    foreach (DynamicBlock dynObject in allDynamicObjects)
                        dynObject.update();
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

        private void spawnVehicle()
        {
            int num = rand.Next(1,100);
            if (num <= Program.settings.Fahrzeuge[0].SpawnRate)
                setVehicle(rand.Next(1000,1005));
            //else if (num < Program.settings.Fahrzeuge[0].SpawnRate + Program.settings.Fahrzeuge[1].SpawnRate)
                //setVehicle(1002);
        }

        private void setVehicle(int tempGID)
        { 
            int randNum = rand.Next(1, spawningPoints.Count());
            Coordiantes coordinates = spawningPoints[randNum-1];
            Vehicle vehicle = new Vehicle() { GID = tempGID, Rotation = coordinates.Rotation, X = coordinates.X, Y = coordinates.Y};
            allDynamicObjects.Add(vehicle);
        }

        public void init()
        {
            map = new TmxMap(mapString, true);
            initSpawningList();
            BlockMapping.Blocks.Count();
            allDynamicObjects = new List<DynamicBlock>();
            //allRoadSigns = new List<RoadSign>();
            //allVehicles = new List<Vehicle>();

            if (map.ObjectGroups.Count > 0)
            {
                foreach (var group in map.ObjectGroups)
                {
                    if (group.Name == "Vehicles")
                    {
                        /*foreach (TmxObject obj in group.Objects)
                        {
                            RoadSign temp = new RoadSign() { Block = obj };
                            temp.Rotation = 10;
                            for (int i = 1; i < 10; i++)
                            {
                                for (int b = 1; b < 10; b++)
                                {
                                    Vehicle vehicle = new Vehicle() { GID = temp.GID, Rotation = temp.Rotation, X = temp.X + (b * 70), Y = temp.Y + (i * 20) };
                                    allDynamicObjects.Add(vehicle);
                                }
                            }
                            return;
                        }*/
                    }
                    else if (group.Name == "Verkehrszeichen")
                    {
                        foreach (TmxObject obj in group.Objects)
                        {
                            RoadSign roadSign = new RoadSign() { Block = obj };
                            if (obj.Tile.Gid == TrafficLight.RedGID)
                                roadSign = new TrafficLight() { Block = obj };
                            //allRoadSigns.Add(roadSign);
                            allDynamicObjects.Add(roadSign);
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
                    spawningPoints.Add(new Coordiantes { X = element.X * 32-64, Y = element.Y*32, Rotation=180});
                else if (element.Gid == (int)StreetDirection.RightToLeft && element.X == map.Width-1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32+96, Y = element.Y * 32+32, Rotation = 0 });
                else if (element.Gid == (int)StreetDirection.TopToBottom && element.Y == 0)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32+32, Y = element.Y * 32 - 64, Rotation = 270 });
                else if (element.Gid == (int)StreetDirection.BottomToTop && element.Y == map.Height-1)
                    spawningPoints.Add(new Coordiantes { X = element.X * 32, Y = element.Y * 32 + 64, Rotation = 90 });
            }
        }
        #endregion
    }
}
