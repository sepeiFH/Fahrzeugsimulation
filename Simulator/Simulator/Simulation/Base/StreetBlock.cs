using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Base
{
    public abstract class StreetBlock : Block
    {
        public StreetDirection Direction { get; set; }
    }

    public abstract class CrossingElement : Block
    {

    }

    public enum StreetDirection
    {
        LeftToRight = 4,
        RightToLeft = 3,
        TopToBottom = 5,
        BottomToTop = 6
    }
}
