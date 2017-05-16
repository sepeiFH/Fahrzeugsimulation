using System;

namespace Traffic_control.Trafficcontrol
{
    public class TrafficLight
    {
        #region Fields
        public LightStatus Status { get; set; }
        public int Group { get; set; }
        public int ID { get; set; }
        public StreetDirection Direction { get; set; }

        private int redPhaseTicks = 100;
        private int yellowPhaseTicks = 40;
        private int greenPhaseTicks = 150;

        private int currentTick = -1;

        public int Duration { get; set; }

        public int Offset { get; set; }

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
        #endregion

        #region Constructor

        public TrafficLight(LightStatus status, int @group, int id, StreetDirection direction)
        {
            Offset = 0;
            Duration = -1;
            Status = status;
            Group = @group;
            ID = id;
            Direction = direction;
        }

        #endregion

        /// <summary>
        /// Changes the state from the actual state to the next state
        /// </summary>
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

        /// <summary>
        /// Check if current tick count is big enough to change the state and change the state if necessary. Also updates the stateChanged reference
        /// </summary>
        /// <param name="stateChanged">Reference to the value which tells if a traffic light has changed his state</param>
        public void update(ref bool stateChanged)
        {
            if (currentTick == Offset + redPhaseTicks || currentTick == Offset + redPhaseTicks + yellowPhaseTicks ||
                currentTick == Offset + redPhaseTicks + yellowPhaseTicks + greenPhaseTicks ||
                currentTick == Offset + redPhaseTicks + 2 * yellowPhaseTicks + greenPhaseTicks)
            {
                changeState();
                stateChanged = true;
            }

            if (currentTick++ > Duration)
                currentTick = 0;
        }
    }
}
