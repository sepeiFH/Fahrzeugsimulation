using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_control.Trafficcontrol
{
    class TrafficLightGroups
    {
        private List<TrafficLight> group1;
        private List<TrafficLight> group2;
        private List<TrafficLight> group3;
        private List<TrafficLight> group4;
        private int durationgrp1 = 5000;
        private int durationgrp2 = 5000;
        private int durationgrp3 = 5000;
        private int durationgrp4 = 5000;
        private int delay = 3000; // time between changes of the several groups
        private int counter = 0;
        private int state;

        /* Mögliche Vorgehensweise:
         * 1) IDs der Ampelblöcke rot, gelbrot, grün, gelb in dieser Reihenfolge über Interface von Simulation einlesen
         * 2) je nachdem welche ID zu welcher Ampel steht, die States bzw. Abfolge dazu definieren
         * 3) Methode mit Timer wird hier sein
         */

        //Constructor
        public TrafficLightGroups()
        {
            group1 = new List<TrafficLight>();
            group2 = new List<TrafficLight>();
            group3 = new List<TrafficLight>();
            group4 = new List<TrafficLight>();
        }

        public int DurationGrp1
        {
            get { return durationgrp1; }
            set { durationgrp1 = value; }
        }

        public int DurationGrp2
        {
            get { return durationgrp2; }
            set { durationgrp2 = value; }
        }

        public int DurationGrp3
        {
            get { return durationgrp3; }
            set { durationgrp3 = value; }
        }
        public int DurationGrp4
        {
            get { return durationgrp4; }
            set { durationgrp4 = value; }
        }

        public int State
        {
            get { return state; }
        }

        public List<TrafficLight> GetGroup1()
        {
            return group1;
        }
        public List<TrafficLight> GetGroup2()
        {
            return group2;
        }
        public List<TrafficLight> GetGroup3()
        {
            return group3;
        }
        public List<TrafficLight> GetGroup4()
        {
            return group4;
        }

        // set light of a traffic light group
        public void setLight(List<TrafficLight> tlgroup, int isGreenState)
        {
            foreach (TrafficLight trlight in tlgroup)
            {
                trlight.IsGreen = isGreenState;
            }
        }

        /// <summary>
        /// Changes the state if the counter reaches the duration of a particular group
        /// </summary>
        /// <returns></returns>
        public bool changeState()
        {
            //On every 1 sec
            counter += 1000;
            if (state == 1)
            {
                if (counter == durationgrp1)
                {
                    // ChangePhaseYellow();
                    ChangePhaseRed();
                    return false;
                }
                else if (counter == durationgrp1 + delay)
                {
                    // ChangePhaseYellow();
                    ChangePhaseGreen();
                    return true;
                }
            }
            else if (state == 2)
            {
                if (counter == durationgrp2)
                {
                    // ChangePhaseYellow();
                    ChangePhaseRed();
                    return false;
                }
                else if (counter == durationgrp2 + delay)
                {
                    // ChangePhaseYellow();
                    ChangePhaseGreen();
                    return true;
                }
            }
            else if (state == 3)
            {
                if (counter == durationgrp3)
                {
                    // ChangePhaseYellow();
                    ChangePhaseRed();
                    return false;
                }
                else if (counter == durationgrp3 + delay)
                {
                    //ChangePhaseYellow();
                    ChangePhaseGreen();
                    return true;
                }
            }
            else if (state == 4)
            {
                if (counter == durationgrp4)
                {
                    ChangePhaseRed();
                    // ChangePhaseYellow();
                    return false;
                }
                else if (counter == durationgrp4 + delay)
                {
                    // ChangePhaseYellow();
                    ChangePhaseGreen();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Switches between the different traffic light groups to red
        /// </summary>
        public void ChangePhaseRed()
        {
            if (state == 1)
            {
                this.setLight(GetGroup1(), state);
            }
            else if (state == 2)
            {
                this.setLight(GetGroup2(), state);
            }
            else if (state == 3)
            {
                this.setLight(GetGroup3(), state);
            }
            else if (state == 4)
            {
                this.setLight(GetGroup4(), state);
            }
        }

        /// <summary>
        /// Switches between the different traffic light groups to green
        /// </summary>
        public void ChangePhaseGreen()
        {
            counter = 0;
            if (state == 1)
            {
                state = 2;
                this.setLight(GetGroup2(), state);
            }
            else if (state == 2)
            {
                state = 3;
                this.setLight(GetGroup3(), state);

            }
            else if (state == 3)
            {
                if (group4.Count == 0)
                {
                    state = 1;
                    this.setLight(GetGroup1(), state);
                }
                else
                {
                    state = 4;
                    this.setLight(GetGroup4(), state);
                }
            }
            else if (state == 4)
            {
                state = 1;
                this.setLight(GetGroup1(), state);
            }
        }

    }
}
