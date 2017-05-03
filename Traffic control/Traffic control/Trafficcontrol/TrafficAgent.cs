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
        private List<TrafficLight> trafficLights;
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

                    if (trafficLights == null)
                        GetTrafficLightData();
                    else
                    {
                        List<TrafficLightGroup> group = TrafficLightGroup.GetAllGroups(trafficLights);

                        foreach (TrafficLightGroup item in group)
                        {
                            /*              Top
                             * ------Left------------Right------
                             *              Bottom
                             * */
                            if (item.TrafficLightCount == 4)
                            {
                                if (item.Left.Duration < 0)
                                {
                                    item.SetDuration(item.Left.TickCount * 2);
                                    // Console.WriteLine("item.Left.TickCount: "+ item.Left.TickCount);

                                    item.Top.Offset = item.Left.TickCount;
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Left.TickCount;
                                    // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);

                                    /* item.Left.Duration = item.Left.TickCount * 2;
                                     Console.WriteLine("item.Left.Duration: " + item.Left.Duration);

                                     item.Right.Duration = item.Left.TickCount * 2;
                                     Console.WriteLine("item.Right.Duration: " + item.Right.Duration);

                                     item.Top.Duration = item.Left.TickCount;
                                     Console.WriteLine("item.Top.Duration: " + item.Top.TickCount);

                                     item.Bottom.Duration = item.Left.TickCount;
                                     Console.WriteLine("item.Left.Duration: " + item.Bottom.TickCount);
                                     */

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
                                    item.SetDuration(item.Left.TickCount * 2);
                                    //item.Right.Offset = item.Left.TickCount;
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Left.TickCount;
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
                             }*/



                            /*             Top
                             * -----Left ---------Right--------
                             *           --------
                             **/
                            /*if (item.TrafficLightCount == 3)
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
                            }*/



                            /*           | Top
                              *          | ---------Right--------
                              *          | Bottom
                              *              
                              **/
                            /*if (item.TrafficLightCount == 3)
                            {
                                if (item.Right.Duration < 0)
                                {
                                    item.SetDuration(item.Left.TickCount * 2);
                                    item.Top.Offset = item.Left.TickCount;
                                    // Console.WriteLine("item.Top.Offset: " + item.Top.Offset);

                                    item.Bottom.Offset = item.Left.TickCount;
                                    // Console.WriteLine("item.Bottom.TickCount: " + item.Bottom.Offset);
                                }
                                item.UpdateTrafficLights();
                            }*/
                            if (stateChanged)
                            {
                                SetUpdatedTrafficLightData();
                                stateChanged = false;
                            }
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

        public void SetUpdatedTrafficLightData()
        {
            List<ServiceReference1.TrafficLightContract> simulationTrafficLights = new List<ServiceReference1.TrafficLightContract>();
            foreach (TrafficLight light in trafficLights)
            {
                simulationTrafficLights.Add(new ServiceReference1.TrafficLightContract(){ ID = light.ID, Status = convertLightStatus(light.Status) });
            }

            clientSimulator.SetTrafficLightUpdate(simulationTrafficLights.ToArray());
        }

        public void GetTrafficLightData()
        {
            var groups = clientSimulator.GetTrafficLightGroups();
            trafficLights = new List<TrafficLight>();
            foreach (var group in groups)
            {
                foreach(var light in group.TrafficLights)
                    trafficLights.Add(new TrafficLight(convertLightStatus(light.Status), group.ID, light.ID, new Point((int)light.PosX, (int)light.PosY)));
            }
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

            private int redPhaseTicks = 100;
            private int yellowPhaseTicks = 40;
            private int greenPhaseTicks = 150;

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

            public void setOffset(TrafficLight trl)
            {
               // trl.Offset 
            }


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
    }
}
