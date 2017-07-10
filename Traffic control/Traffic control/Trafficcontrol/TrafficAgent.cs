using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        /// <summary>
        /// Creates the traffic agent task from Task Factory
        /// </summary>
        public Task GetTrafficAgentTask()
        {
            token = new CancellationTokenSource();
            return Task.Factory.StartNew(() => TickLogic(token.Token));
        }

        /// <summary>
        /// Starts the traffic agent task
        /// </summary>
        public void StartTrafficAgent()
        {
            if (logicTask == null)
            {
                logicTask = GetTrafficAgentTask();
            }
        }

        /// <summary>
        /// Stops the traffic agent task
        /// </summary>
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

        /// <summary>
        /// Query the initial data of the simulation interface (map, config(TODO))
        /// Starts a Stopwatch for the clock, so that there is the given amount of ticks within a second.
        /// Also calls the update method of all traffic light objects and send the updated objects to the simulation.
        /// </summary>
        /// <param name="cancelToken">Token which tells the working loop that the program should stop</param>
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
                    //Task.Delay(10);
                }

            }

        }

        /// <summary>
        /// Convert the status of the traffic lights from the interface type into the internal type
        /// </summary>
        /// <param name="status">light status to be converted (from interface type to internal type)</param>
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

        /// <summary>
        /// Convert the status of the traffic lights from the internal type into the interface type
        /// </summary>
        /// <param name="status">light status to be converted (from internal type to interface type)</param>
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

        /// <summary>
        /// Convert the position of the traffic lights from the interface type into the internal type
        /// </summary>
        /// <param name="oldPosition">light position to be converted (from interface type to internal type)</param>
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

        /// <summary>
        /// Convert the internal traffic lights into traffic lights of the interface type and PUSH the traffic lights via the interface to the simulation
        /// </summary>
        public void SetUpdatedTrafficLightData()
        {
            List<ServiceReference1.TrafficLightContract> simulationTrafficLights = new List<ServiceReference1.TrafficLightContract>();
            foreach (TrafficLightGroup group in trafficLightGroups)
            {
                foreach (List<TrafficLight> lights in group.LightDict.Values)
                    foreach (TrafficLight light in lights)
                        simulationTrafficLights.Add(new ServiceReference1.TrafficLightContract() { ID = light.ID, Status = convertLightStatus(light.Status) });
            }
            try
            {
                clientSimulator.SetTrafficLightUpdate(simulationTrafficLights.ToArray());
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// PULL the traffic lights from the simulation interface and convert them into internal traffic lights
        /// TODO: also PULL the config data
        /// </summary>
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
        }

        #endregion Agent Ticks
    }
}
