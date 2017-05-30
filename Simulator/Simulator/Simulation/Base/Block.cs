using System;
using TiledSharp;

namespace Simulator.Simulation.Base
{
    /// <summary>
    /// Basic Element for the simulation and the GUI
    /// </summary>
    public abstract class Block : IComparable<Block>
    {
        public int GID { get; set; }

        public BlockType Type { get; set; }

        public int CompareTo(Block other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var gidComparison = GID.CompareTo(other.GID);
            if (gidComparison != 0) return gidComparison;
            return Type.CompareTo(other.Type);
        }
    }
    /// <summary>
    /// Enum for the two needed blocktypes: staticBlocks can't move or change their behavior, dynamic blocks do that.
    /// </summary>
    public enum BlockType
    {
        StaticBlock,
        DynamicBlock
    }

    /// <summary>
    /// Basic Class for most of the dynamic blocks, it adds the TmxObject to the dynamic block. This enables the transmission and usage for the GUI
    /// </summary>
    public abstract class BlockObject : DynamicBlock
    {
        public TmxObject Block { get; set; }

        public override double X
        {
            get { return Block.X; }
            set { if (Block != null) Block.X = value; }
        }

        public override double Y
        {
            get { return Block.Y; }
            set { if (Block != null) Block.Y = value; }
        }

        public override double Rotation
        {
            get { return Block.Rotation; }
            set { if (Block != null) Block.Rotation = value; }
        }

        public override int GID
        {
            get { return Block.Tile.Gid; }
            set { if (Block != null) Block.Tile.Gid = value; }
        }
    }
}
