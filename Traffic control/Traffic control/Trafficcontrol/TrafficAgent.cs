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
                            item.SetDuration();
                            item.setOffset();
                            item.UpdateTrafficLights(ref stateChanged);
                        }
                        if (stateChanged)
                        {
                            SetUpdatedTrafficLightData();
                            stateChanged = false;
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
    }
}
