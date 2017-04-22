using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Simulator.Simulation.Base;
using TiledSharp;

namespace Simulator.Simulation
{
    public class Simulator
    {
        #region Fields
        public readonly int RefreshesPerSecond = 50;
        private readonly TimeSpan SimulatorSpeed;
        private Stopwatch stopWatch;

        public TmxMap map;
        //public List<RoadSign> allRoadSigns;
        //public List<Vehicle> allVehicles;

        public List<DynamicBlock> allDynamicObjects;

        public string mapString = File.ReadAllText("Map/FH3.tmx");

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

        public void init()
        {
            map = new TmxMap(mapString, true);
            BlockMapping.Blocks.Count();
            allDynamicObjects = new List<DynamicBlock>();
            //allRoadSigns = new List<RoadSign>();
            //allVehicles = new List<Vehicle>();

            if (map.ObjectGroups.Count > 0)
            {
                foreach (TmxObject obj in map.ObjectGroups[0].Objects)
                {
                    RoadSign roadSign = new RoadSign() { Block = obj };
                    if (obj.Tile.Gid == TrafficLight.RedGID)
                        roadSign = new TrafficLight() { Block = obj };
                    //allRoadSigns.Add(roadSign);
                    allDynamicObjects.Add(roadSign);
                }

                foreach (TmxObject obj in map.ObjectGroups[1].Objects)
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
                }
            }
        }
        #endregion
    }
}
