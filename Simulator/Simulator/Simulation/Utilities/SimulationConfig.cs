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
    }

    class SimulationConfig
    {
        public String BaseUrl { get; set; }
        public List<Fahrzeug> Fahrzeuge { get; set; }
    }
}
