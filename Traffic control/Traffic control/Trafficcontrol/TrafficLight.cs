using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_control.Trafficcontrol
{
    class TrafficLight
    {
        // private members
        private int duration;
        private int id;
        private int state; // 1 = green, 2 = yellow, 3 = red
        private bool isGreen;
        private bool isYellow;


        public TrafficLight()
        {
            this.duration = 5;
            this.isGreen = false;
            this.isYellow = false;
        }

        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public bool IsGreen
        {
            get { return isGreen; }
            set { isGreen = value; }
        }
        public bool IsYellow
        {
            get { return isYellow; }
            set { isYellow = value; }
        }

      /*  public int getState()
        {
            return state;
        }

        public int setState(int s)
        {
            return state = s;
        }
        */
    }
}
