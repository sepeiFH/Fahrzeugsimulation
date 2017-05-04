using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_control.Trafficcontrol
{
    public class TrafficLight
    {
        public LightStatus Status { get; set; }
        public int Group { get; set; }

        public int ID { get; set; }

        public StreetDirection Direction { get; set; }

        private int redPhaseTicks = 100;
        private int yellowPhaseTicks = 40;
        private int greenPhaseTicks = 150;

        public int TickCount
        {
            get { return redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks; }
        }

        public enum LightStatus
        {
            Red = 12,
            Yellow = 13,
            Green = 14,
            YellowRed = 15
        }
        public enum StreetDirection
        {
            All,
            Straight,
            Right,
            Left
        }
        #region Constructor

        public TrafficLight(LightStatus status, int @group, int id, StreetDirection direction)
        {
            Status = status;
            Group = @group;
            ID = id;
            Direction = direction;
        }

        #endregion

        private void changeState()
        {
            switch (Status)
            {
                case LightStatus.Red:
                    Status = LightStatus.YellowRed;
                    break;
                case LightStatus.YellowRed:
                    Status = LightStatus.Green;
                    break;
                case LightStatus.Green:
                    Status = LightStatus.Yellow;
                    break;
                case LightStatus.Yellow:
                    Status = LightStatus.Red;
                    break;
            }
            Console.WriteLine("Ampel " + ID + " hat auf " + Status + " geschalten");
        }

        private int currentTick = -1;
        private int offset = 0;
        private int duration = -1;

        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public void update(ref bool stateChanged)
        {
            if (currentTick == offset + redPhaseTicks || currentTick == offset + redPhaseTicks + yellowPhaseTicks ||
                currentTick == offset + redPhaseTicks + yellowPhaseTicks + greenPhaseTicks ||
                currentTick == offset + redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks)
            {
                changeState();
                stateChanged = true;
            }

            if (currentTick++ > Duration)
                currentTick = 0;
        }
    }
}
