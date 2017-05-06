using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Base
{
    public abstract class DynamicBlock : Block
    {
        public abstract double X { get; set; }
        public abstract double Y { get; set; }
        public abstract double Rotation { get; set; }

        public abstract int GID { get; set; }
        public abstract int ID { get; set; }

        public abstract bool IsBroken { get; set; }

        public abstract void update();
    }
}
