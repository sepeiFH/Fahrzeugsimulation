using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Utilities
{
    class Fahrzeug
    {
        public string Type { get; set; }
        public int SpawnRate { get; set; }
        public int GID { get; set; }
    }

    class SimulationConfig
    {
        public int Takt { get; set; }
        public String BaseUrl { get; set; }
        public List<Fahrzeug> Fahrzeuge { get; set; }
    }

    class CrossingConfig
    {
        public int Takt { get; set; }
    }
}
