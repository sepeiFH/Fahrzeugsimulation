using System;
using System.Collections.Generic;

namespace Simulator.Simulation.Utilities
{
    public class ConfigVehicle
    {
        private double maxVelocity;
        public double MaxVelocity { get { return maxVelocity; } set { maxVelocity = value > 100 ? value / 10 : value; } }
        private double maxAcceleration;
        public double MaxAcceleration { get { return maxAcceleration; } set { maxAcceleration = value > 100 ? value / 10 : value; } }
        private double maxDeceleration;
        public double MaxDeceleration { get { return maxDeceleration; } set { maxDeceleration = value > 100 ? value / 10 : value; } }
        public double Length { get; set; }
        public double Width { get; set; }
        public string Type { get; set; }
        public int SpawnRate { get; set; }
        public int GID { get; set; }
    }

    public class SimulationConfig
    {
        public int SpawnTimeFrame { get; set; }
        public int TurningSpeed { get; set; }
        public int EmergencyTime { get; set; }
        public int Takt { get; set; }
        public String BaseUrl { get; set; }
        public List<ConfigVehicle> Vehicles { get; set; }
    }

    class CrossingConfig
    {
        public int Takt { get; set; }
    }
}
