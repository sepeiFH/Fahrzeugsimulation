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
        private int isGreen;
        private int isYellow;
        private int isRed;


        public TrafficLight()
        {
            this.duration = 5;
            this.isGreen = 1;
            this.isYellow = 2;
            this.isRed = 3;
        }

        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        public int IsGreen
        {
            get { return isGreen; }
            set { isGreen = value; }
        }
        public int IsYellow
        {
            get { return isYellow; }
            set { isYellow = value; }
        }

        public int IsRed
        {
            get { return isRed; }
            set { isRed = value; }
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
