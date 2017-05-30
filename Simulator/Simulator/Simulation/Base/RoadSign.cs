using System;
using TiledSharp;

namespace Simulator.Simulation.Base
{
    /// <summary>
    /// Basic Class for all roadsigns. 
    /// </summary>
    public class RoadSign : BlockObject
    {
        public int RoadSignGroup
        {
            get
            {
                if (Block != null)
                {
                    foreach (var item in Block.Properties)
                        if (item.Key == "Group")
                            return Convert.ToInt32(item.Value);
                }

                return -1;
            }
        }

        public override int ID { get; set; }
        public override bool IsBroken { get; set; }

        public RoadSign()
        {
            GID = -1;
            Type = BlockType.StaticBlock;
        }

        public override void update()
        {
        }
    }

    /// <summary>
    /// Implementation of the RoadSign class for the traffic light objects of the simulation and GUI.
    /// </summary>
    public class TrafficLight : RoadSign
    {
        public static int RedGID = 12;
        public LightStatus Status { get; set; }

        /// <summary>
        /// Enum for the different light states. enum numbers equals the GID of the GUI-block
        /// </summary>
        public enum LightStatus
        {
            Red = 12,
            Yellow = 13,
            Green = 14,
            YellowRed = 15,
            Off = 16
        }

        public TmxObject Block
        {
            get
            {
                TmxObject temp = base.Block;
                temp.Tile.Gid = (int)Status;
                return temp;
            }

            set { base.Block = value; }
        }

        public TrafficLight()
        {
            Type = BlockType.DynamicBlock;
            Status = LightStatus.Red;
        }

        private int emergencyPhaseTicks = 30;

        /// <summary>
        /// MEthod to change the traffic lights state while the traffic control is not available and simulation is in emergency-traffic-control-mode
        /// </summary>
        private void changeState()
        {
            //Console.WriteLine("Ampel State Changed");
            if (Status == LightStatus.Red)
            {
                Status = LightStatus.Yellow;
                base.Block = Block;
                return;
            }
            if (Status == LightStatus.YellowRed)
            {
                Status = LightStatus.Yellow;
                base.Block = Block;
                return;
            }
            if (Status == LightStatus.Green)
            {
                Status = LightStatus.Yellow;
                base.Block = Block;
                return;
            }
            if (Status == LightStatus.Off)
            {
                Status = LightStatus.Yellow;
                base.Block = Block;
                return;
            }
            if (Status == LightStatus.Yellow)
            {
                Status = LightStatus.Off;
                base.Block = Block;
                return;
            }
        }

        private int currentTick = 0;
        public override void update()
        {
            if (Simulator.EmergencyModeActive)
            {
                if (++currentTick == emergencyPhaseTicks)
                {
                    changeState();
                    currentTick = 0;
                }
            }
        }
    }
}
