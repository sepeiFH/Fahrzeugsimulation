using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Traffic_control.Trafficcontrol
{
    class TrafficAgent
    {

        #region Fields

        public readonly int RefreshesPerSecond = 50;
        private readonly TimeSpan TrafficAgentSpeed;
        private Stopwatch stopWatch;
        private Task logicTask;
        private CancellationTokenSource token;
        private List<TrafficLightGroup> trafficLightGroups;
        private static bool stateChanged;

        private ServiceReference1.SimulatorServiceTrafficControlClient clientSimulator;

        #endregion Fields

        #region Constructor

        public TrafficAgent()
        {
            TrafficAgentSpeed = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / RefreshesPerSecond);
        }

        #endregion Constructor

        #region Agent Thread

        public Task GetTrafficAgentTask()
        {
            token = new CancellationTokenSource();
            return Task.Factory.StartNew(() => TickLogic(token.Token));
        }

        public void StartTrafficAgent()
        {
            if (logicTask == null)
            {
                logicTask = GetTrafficAgentTask();
            }
        }

        public void StopTrafficAgent()
        {
            if (logicTask != null)
            {
                token.Cancel(false);
                logicTask = null;
            }
        }

        #endregion  Agent Thread

        #region Agent Ticks

        private void TickLogic(CancellationToken cancelToken)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();

            clientSimulator = new ServiceReference1.SimulatorServiceTrafficControlClient();
            stateChanged = false;

            while (true)
            {
                if (stopWatch.Elapsed > TrafficAgentSpeed)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    if (trafficLightGroups == null)
                        GetTrafficLightData();
                    else
                    {
                        foreach (TrafficLightGroup item in trafficLightGroups)
                        {
<<<<<<< HEAD
                            /*              Top
                             * ------Left------------Right------
                             *              Bottom
                             * */
                            if (item.TrafficLightCount == 4)
                            {
                                if (item.Left.Duration < 0)
                                {
                                    //Console.WriteLine("OK! ");
                                    item.SetDuration(item.Left.TickCount * 2);
                                    // Console.WriteLine("item.Left.TickCount: "+ item.Left.TickCount);

                                    // item.setOffset(150);

                                    item.Top.Offset = item.Left.TickCount;
                                    //Console.WriteLine("item.ID: " + item.ID);
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Left.TickCount;
                                    //Console.WriteLine("item.Bottom.Offset: " + item.Bottom.Offset);

                                    item.Right.Duration = item.Left.TickCount * 2;
                                    // Console.WriteLine("item.Right.Duration: " + item.Right.Duration);

                                    // item.Top.Duration = item.Left.TickCount;
                                    //Console.WriteLine("item.Top.Duration: " + item.Top.TickCount);

                                    //item.Bottom.Duration = item.Left.TickCount;
                                    //Console.WriteLine("item.Left.Duration: " + item.Bottom.TickCount);
                                }
                                item.UpdateTrafficLights();
                            }

                            /*              ---------------
                             *    -------Left ---------------Right--------
                             *                  Bottom
                             *              
                             **/
                            if (item.TrafficLightCount == 3)
                            {
                                if (item.Left.Duration < 0)
                                {
                                    // Console.WriteLine("TrafficLightCount = 3!!!!!");
                                    //item.setOffset(150);
                                    item.SetDuration(item.Left.TickCount * 2);
                                    item.Right.Duration = item.Left.TickCount * 2;
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Left.TickCount;
                                    //item.Bottom.Duration = item.Left.TickCount;
                                    // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);
                                }
                                item.UpdateTrafficLights();
                            }

                            /*                  Top    |
                            *    -------Left --------- |
                            *                  Bottom  |
                            *              
                            **/
                            /* if (item.TrafficLightCount == 3)
                             {
                                 if (item.Left.Duration < 0)
                                 {
                                     item.SetDuration(item.Left.TickCount * 2);
                                     item.Top.Offset = item.Left.TickCount;
                                     // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                     item.Bottom.Offset = item.Left.TickCount;
                                     // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);
                                 }
                                 item.UpdateTrafficLights();
                             }
                             
                            /*             Top
                             * -----Left ---------Right--------
                             *           --------
                             **/
                            /*  if (item.TrafficLightCount == 3)
                              {
                                  if (item.Left.Duration < 0)
                                  {
                                      item.SetDuration(item.Left.TickCount * 2);
                                      item.Top.Offset = item.Left.TickCount;
                                      // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                      item.Bottom.Offset = item.Left.TickCount;
                                      // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);
                                  }
                                  item.UpdateTrafficLights();
                              }

                              /*           | Top
                                *          | ---------Right--------
                                *          | Bottom
                                *              
                                **/
                            /*if (item.TrafficLightCount == 3)
                            {
                                if (item.Right.Duration < 0)
                                {
                                    item.SetDuration(item.Right.TickCount * 2);
                                    item.Top.Offset = item.Right.TickCount;
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Right.TickCount;
                                    // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);
                                }
                                item.UpdateTrafficLights();
                            }*/
                            if (item.TrafficLightCount == 2)
                            {
                                if (item.Left.Duration < 0)
                                {
                                    // Console.WriteLine("item = 2-----");
                                    item.SetDuration(item.Left.TickCount * 2);
                                    item.Right.Duration = item.Left.TickCount * 2;
                                }
                                item.UpdateTrafficLights();
                            }
                            if (stateChanged)
                            {
                                SetUpdatedTrafficLightData();
                                stateChanged = false;
                            }
=======
                            item.SetDuration();
                            item.setOffset();
                            item.UpdateTrafficLights(ref stateChanged);
                        }
                        if (stateChanged)
                        {
                            SetUpdatedTrafficLightData();
                            stateChanged = false;
>>>>>>> origin/newTrafficControl
                        }
                    }
                    stopWatch.Restart();
                }
                else
                {
                    Task.Delay(10);
                }
                
            }

        }

        private TrafficLight.LightStatus convertLightStatus(ServiceReference1.TrafficLightStatus status)
        {
            TrafficLight.LightStatus newStatus;

            if (status == ServiceReference1.TrafficLightStatus.Green)
                newStatus = TrafficLight.LightStatus.Green;
            else if (status == ServiceReference1.TrafficLightStatus.Red)
                newStatus = TrafficLight.LightStatus.Red;
            else if (status == ServiceReference1.TrafficLightStatus.Yellow)
                newStatus = TrafficLight.LightStatus.Yellow;
            else
                newStatus = TrafficLight.LightStatus.YellowRed;

            return newStatus;
        }
        private ServiceReference1.TrafficLightStatus convertLightStatus(TrafficLight.LightStatus status)
        {
            ServiceReference1.TrafficLightStatus newStatus;

            if (status == TrafficLight.LightStatus.Green)
                newStatus = ServiceReference1.TrafficLightStatus.Green;
            else if (status == TrafficLight.LightStatus.Red)
                newStatus = ServiceReference1.TrafficLightStatus.Red;
            else if (status == TrafficLight.LightStatus.Yellow)
                newStatus = ServiceReference1.TrafficLightStatus.Yellow;
            else
                newStatus = ServiceReference1.TrafficLightStatus.YellowRed;

            return newStatus;
        }
        private TrafficLightGroup.Position convertLightPosition(ServiceReference1.TrafficLightPosition oldPosition)
        {
            TrafficLightGroup.Position newPosition;

            if (oldPosition == ServiceReference1.TrafficLightPosition.Top)
                newPosition = TrafficLightGroup.Position.Top;
            else if (oldPosition == ServiceReference1.TrafficLightPosition.Bottom)
                newPosition = TrafficLightGroup.Position.Bottom;
            else if (oldPosition == ServiceReference1.TrafficLightPosition.Left)
                newPosition = TrafficLightGroup.Position.Left;
            else
                newPosition = TrafficLightGroup.Position.Right;
            return newPosition;
        }

        public void SetUpdatedTrafficLightData()
        {
            List<ServiceReference1.TrafficLightContract> simulationTrafficLights = new List<ServiceReference1.TrafficLightContract>();
            foreach (TrafficLightGroup group in trafficLightGroups)
            {
                foreach (List<TrafficLight> lights in group.LightDict.Values)
                    foreach (TrafficLight light in lights)
                        simulationTrafficLights.Add(new ServiceReference1.TrafficLightContract() { ID = light.ID, Status = convertLightStatus(light.Status) });
            }

            clientSimulator.SetTrafficLightUpdate(simulationTrafficLights.ToArray());
        }

        public void GetTrafficLightData()
        {
            var groups = clientSimulator.GetTrafficLightGroups();
            trafficLightGroups = new List<TrafficLightGroup>();
            foreach (var group in groups)
            {
                TrafficLightGroup tempGroup = new TrafficLightGroup() { LightDict = new Dictionary<TrafficLightGroup.Position, List<TrafficLight>>() };
                tempGroup.ID = group.ID;

                foreach (var light in group.TrafficLights)
                {
                    if (!tempGroup.LightDict.ContainsKey(convertLightPosition(light.Position)))
                        tempGroup.LightDict.Add(convertLightPosition(light.Position), new List<TrafficLight>() { new TrafficLight(convertLightStatus(light.Status), group.ID, light.ID, TrafficLight.StreetDirection.All) });
                    else
                        tempGroup.LightDict[convertLightPosition(light.Position)].Add(new TrafficLight(convertLightStatus(light.Status), group.ID, light.ID, TrafficLight.StreetDirection.All));
                }

                trafficLightGroups.Add(tempGroup);
            }
            /*var groups = clientSimulator.GetTrafficLightGroups();
            trafficLights = new List<TrafficLight>();
            foreach (var group in groups)
            {
                foreach(var light in group.TrafficLights)
                    trafficLights.Add(new TrafficLight(convertLightStatus(light.Status), group.ID, light.ID, TrafficLight.StreetDirection.All));
            }*/
            /*
            trafficLights = new List<TrafficLight>()
            {
                new TrafficLight(TrafficLight.LightStatus.Red, 1, 1, new Point(0, 16)),
                new TrafficLight(TrafficLight.LightStatus.Red, 1, 2, new Point(16, 32)),
                new TrafficLight(TrafficLight.LightStatus.Red, 1, 3, new Point(32, 16)),
                new TrafficLight(TrafficLight.LightStatus.Red, 1, 4, new Point(16, 0)),
                new TrafficLight(TrafficLight.LightStatus.Red, 2, 5, new Point(-1, -1)),
                //new TrafficLight(TrafficLight.LightStatus.Red, 2, 6),
                //new TrafficLight(TrafficLight.LightStatus.Red, 2, 7)
            };*/
        }

        #endregion Agent Ticks
<<<<<<< HEAD

        #region TrafficLight

        public class TrafficLight
        {
            public LightStatus Status { get; set; }
            public int Group { get; set; }

            public int ID { get; set; }

            public Point Position { get; set; }
            public enum LightStatus
            {
                Red = 12,
                Yellow = 13,
                Green = 14,
                YellowRed = 15
            }

            #region Constructor

            public TrafficLight(LightStatus status, int @group, int id, Point position)
            {
                Status = status;
                Group = @group;
                ID = id;
                Position = position;
            }

            private int redPhaseTicks = 130;
            private int yellowPhaseTicks = 100;
            private int greenPhaseTicks = 200;

            #endregion

            public int TickCount
            {
                get { return redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks; }
            }

            private void changeState()
            {
                switch (Status)
                {
                    case LightStatus.Red:
                        Status = LightStatus.YellowRed;
                        break;
                    case LightStatus.YellowRed:
                        Status = LightStatus.Green;
                        break;
                    case LightStatus.Green:
                        Status = LightStatus.Yellow;
                        break;
                    case LightStatus.Yellow:
                        Status = LightStatus.Red;
                        break;
                }
                stateChanged = true;
                Console.WriteLine("Ampel " + ID + " hat auf " + Status + " geschalten");
            }

            private int currentTick = -1;
            private int offset = 0;
            private int duration = -1;

            public int Duration
            {
                get { return duration; }
                set { duration = value; }
            }

            public int Offset
            {
                get { return offset; }
                set { offset = value; }
            }

            public void update()
            {

                if (currentTick == offset + redPhaseTicks || currentTick == offset + redPhaseTicks + yellowPhaseTicks ||
                    currentTick == offset + redPhaseTicks + yellowPhaseTicks + greenPhaseTicks ||
                    currentTick == offset + redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks)
                {
                    /*Console.WriteLine("currenttick: " + currentTick);
                    Console.WriteLine("offset: " + offset);
                    Console.WriteLine("offset + redphaseTick: " + (offset + redPhaseTicks));
                    Console.WriteLine("offset + redphaseTick + yellowPhaseTicks: " + (offset + redPhaseTicks + yellowPhaseTicks));
                    Console.WriteLine("offset + redphaseTick + yellowPhaseTicks + greenPhaseTicks: " + (offset + redPhaseTicks + yellowPhaseTicks + greenPhaseTicks));
                    Console.WriteLine("offset + redphaseTick + 2*yellowPhaseTicks + greenPhaseTicks: " + (offset + redPhaseTicks + (2 * yellowPhaseTicks) + greenPhaseTicks));
                    */
                    changeState();
                }

                if (currentTick++ > Duration)
                    currentTick = 0;
            }
        }

        #endregion TrafficLight

        #region TrafficLightGroup

        public class TrafficLightGroup
        {
            public int ID { get; set; }
            private List<TrafficLight> trafficLights;

           
            public TrafficLight Left
            {
                get { return trafficLights.FirstOrDefault(x => x.Position.X == trafficLights.Min(y => y.Position.X)); }
            }

            public TrafficLight Right
            {
                get { return trafficLights.FirstOrDefault(x => x.Position.X == trafficLights.Max(y => y.Position.X)); }
            }
            public TrafficLight Top
            {
                get { return trafficLights.FirstOrDefault(x => x.Position.Y == trafficLights.Min(y => y.Position.Y)); }
            }

            public TrafficLight Bottom
            {
                get { return trafficLights.FirstOrDefault(x => x.Position.Y == trafficLights.Max(y => y.Position.Y)); }
            }

            public int TrafficLightCount
            {
                get { return trafficLights.Count; }
            }

            private static TrafficLightGroup GetTrafficLightGroup(int id, List<TrafficLight> allLights)
            {
                return new TrafficLightGroup { ID = id, trafficLights = allLights.Where(x => x.Group == id).ToList() };
            }

            public static List<TrafficLightGroup> GetAllGroups(List<TrafficLight> allLights)
            {
                List<TrafficLightGroup> temp = new List<TrafficLightGroup>();
                var list = allLights.GroupBy(x => x.Group).Select(y => y.First()).ToList();

                foreach (var item in list)
                    temp.Add(GetTrafficLightGroup(item.ID, allLights));

                return temp;
            }


           /* public void setOffset(int offset)
            {
                List<TrafficLightGroup> group = TrafficLightGroup.GetAllGroups(trafficLights);
                foreach (var item in group)
                {
                    if (item.TrafficLightCount == 4)
                    {
                        item.Top.Offset = offset;
                        item.Bottom.Offset = offset;
                    }
                    if (item.TrafficLightCount == 3)
                    {
                        item.Bottom.Offset = offset;
                    }
                     if (item.TrafficLightCount == 3)
                     {
                         item.Left.Offset = offset;
                     }
                }
            } */  

            public void SetDuration(int duration)
            {
                foreach (var item in trafficLights)
                    item.Duration = duration;
            }

            public void UpdateTrafficLights()
            {
                foreach (var item in trafficLights)
                    item.update();
            }
        }



        #endregion TrafficLightGroup
=======
>>>>>>> origin/newTrafficControl
    }
}
