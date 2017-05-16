using System.Collections.Generic;

namespace Simulator.Simulation.Base
{
    public abstract class StreetBlock : Block
    {
        public StreetDirection Direction { get; set; }
    }

    public class StreetWithVehicle : StreetBlock
    {

    }

    public abstract class CrossingElement : StreetBlock
    {
        public List<CrossingDirection> PossibleCrosDirs { get; set; }
    }
    public class CrossingBlock : CrossingElement
    {
        public int posX { get; set; }
        public int posY { get; set; }
        public CrossingBlock()
        {
            Type = BlockType.StaticBlock;
        }
    }

    public enum StreetDirection
    {
        RightToLeft = 3,
        LeftToRight = 4,
        TopToBottom = 5,
        BottomToTop = 6,
        Crossing = 7
    }
    public enum CrossingDirection
    {
        Left,
        Right,
        Straight,
        None
    }
}
