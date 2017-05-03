using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TiledSharp;

namespace Simulator.Simulation.Base
{
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

        public RoadSign()
        {
            GID = -1;
            Type = BlockType.StaticBlock;
        }

        public override void update()
        {
        }
    }

    public class TrafficLight : RoadSign
    {
        public static int RedGID = 12;
        public LightStatus Status { get; set; }

        public enum LightStatus
        {
            Red = 12,
            Yellow = 13,
            Green = 14,
            YellowRed = 15
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

        public int startOffset = 0;
        public int redPhaseTicks = 50;
        private int yellowPhaseTicks = 20;
        private int greenPhaseTicks = 60;

        private void changeState()
        {
            //Console.WriteLine("Ampel State Changed");
            if (Status == LightStatus.Red)
            {
                Status = LightStatus.YellowRed;
                base.Block = Block;
                return;
            }

            if (Status == LightStatus.YellowRed)
            {
                Status = LightStatus.Green;
                base.Block = Block;
                return;
            }

            if (Status == LightStatus.Green)
            {
                Status = LightStatus.Yellow;
                base.Block = Block;
                return;
            }

            if (Status == LightStatus.Yellow)
            {
                Status = LightStatus.Red;
                base.Block = Block;
                return;
            }
        }

        private int currentTick = -1;
        public override void update()
        {
            /*
            if (currentTick < 0)
                currentTick = startOffset;

            if (currentTick == redPhaseTicks || currentTick == redPhaseTicks + yellowPhaseTicks ||
                currentTick == redPhaseTicks + yellowPhaseTicks + greenPhaseTicks ||
                currentTick == redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks)
                changeState();

            if (currentTick++ > redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks)
                currentTick = 0;
                */
            /*if (stopWatch == null)
            {
                stopWatch = new StopWatchWithOffset(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * startOffsetSecond));
                stopWatch.Start();
            }*/
        }
    }
}
