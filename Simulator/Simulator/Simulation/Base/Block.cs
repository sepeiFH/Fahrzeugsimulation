using Simulator.Simulation.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using TiledSharp;

namespace Simulator.Simulation.Base
{
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

    public abstract class DynamicBlock : Block
    {
        public abstract double X { get; set; }
        public abstract double Y { get; set; }
        public abstract double Rotation { get; set; }

        public abstract int GID { get; set; }

        public abstract void update();
    }


    public abstract class StreetBlock : Block
    {
        public StreetDirection Direction { get; set; }
    }

    public abstract class CrossingElement : Block
    {
        
    }

    public enum StreetDirection
    {
        LeftToRight,
        RightToLeft,
        TopToDown,
        DownToTop
    }

    public enum BlockType
    {
        StaticBlock,
        DynamicBlock
    }

    public abstract class BlockObject : DynamicBlock
    {
        public TmxObject Block { get; set; }

        public override double X
        {
            get { return Block.X; }
            set { if(Block != null) Block.X = value; }
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

    public class RoadSign : BlockObject
    {
        public int RoadSignGroup
        {
            get
            {
                if (Block != null)
                {
                    foreach(var item in Block.Properties)
                        if (item.Key == "Group")
                            return Convert.ToInt32(item.Value);
                }

                return -1;
            }
        }
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
                temp.Tile.Gid = (int) Status;
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
            if (currentTick < 0)
                currentTick = startOffset;

            if (currentTick == redPhaseTicks || currentTick == redPhaseTicks + yellowPhaseTicks ||
                currentTick == redPhaseTicks + yellowPhaseTicks + greenPhaseTicks ||
                currentTick == redPhaseTicks + 2*yellowPhaseTicks + greenPhaseTicks)
                changeState();

            if (currentTick++ > redPhaseTicks +2*yellowPhaseTicks + greenPhaseTicks)
                currentTick = 0;

            /*if (stopWatch == null)
            {
                stopWatch = new StopWatchWithOffset(TimeSpan.FromTicks(TimeSpan.TicksPerSecond * startOffsetSecond));
                stopWatch.Start();
            }*/
        }
    }

    public class Vehicle : DynamicBlock
    {
        public override double X { get; set; }
        public override double Y { get; set; }
        public override double Rotation { get; set; }
        public override int GID { get; set; }

        public enum VehicleList
        {
            LKW1 = 37,
            Car1 = 38,
        }
        public Vehicle()
        {
        }

        private Dictionary<VehicleList, int> vehicleLengths = new Dictionary<VehicleList, int>() { {VehicleList.LKW1, 60 }, { VehicleList.Car1, 32 } };
        private int count;
        public override void update()
        {
            if (X < -100)
                X = 2000;
            else if (X > 2000)
                X = -100;

            if (Y < -100)
                Y = 1000;
            if (Y > 1000)
                Y = -100;

            MoveVehicle(5);
            if (count++ % 2 == 0)
            {
                this.Rotation += 0.25;
                count = 2;
            }
                
        }

        //Clockwise Rotation: Left Startpoint 90° Top 180 Right 270 Down
        public void MoveVehicle(double doublePixels)
        {
            double yy = Y + doublePixels;
            double angle = (Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            double newX = (Math.Cos(radiants) * (double)(X - X) - Math.Sin(radiants) * (double)(yy - Y) + X);
            double newY = (Math.Sin(radiants) * (double)(X - X) + Math.Cos(radiants) * (double)(yy - Y) + Y);

            X = newX;
            Y = newY;
        }
    }


}