using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Utilities
{
    public class ConfigVehicle
    {
        public double MaxVelocity { get; set; }
        public double MaxAcceleration { get; set; }
        public double MaxDeceleration { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public string Type { get; set; }
        public int SpawnRate { get; set; }
        public int GID { get; set; }
    }

    public class SimulationConfig
    {
        public int SpawnTimeFrame { get; set; }
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
