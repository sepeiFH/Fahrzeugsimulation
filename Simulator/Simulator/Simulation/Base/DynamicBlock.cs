namespace Simulator.Simulation.Base
{
    /// <summary>
    /// Basic Class for most of the dynamic blocks, it defines the minimal Fields which every dynamic block needs
    /// </summary>
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
