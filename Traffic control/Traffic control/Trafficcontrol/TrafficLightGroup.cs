using System.Collections.Generic;
using System.Linq;

namespace Traffic_control.Trafficcontrol
{
    public class TrafficLightGroup
    {
        public int ID { get; set; }

        public Dictionary<Position, List<TrafficLight>> LightDict { get; set; }

        public enum Position
        {
            Top,
            Bottom,
            Right,
            Left
        }

        /// <summary>
        /// Sets the Offset of a traffic light group, depending on the amount of traffic lights in the group
        /// </summary>
        public void setOffset()
        {
            int lightDuration = LightDict.Values.First().First().TickCount;
            // 4 traffic lights
            if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
            {
                foreach (TrafficLight light in LightDict[Position.Right])
                    light.Offset = lightDuration;
                foreach (TrafficLight light in LightDict[Position.Left])
                    light.Offset = lightDuration;
            }
            // 3 traffic lights, Top does not exist
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Bottom])
                    light.Offset = lightDuration;
            // 3 traffic lights, Bottom does not exist
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top))
                foreach (TrafficLight light in LightDict[Position.Top])
                    light.Offset = lightDuration;
            // 3 traffic lights, Left does not exist
            else if (LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Right])
                    light.Offset = lightDuration;
            // 3 traffic lights, Right does not exist
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Left])
                    light.Offset = lightDuration;
        }

        /// <summary>
        /// Sets the Duration of all traffic lights in the group
        /// </summary>
        public void SetDuration()
        {
            int lightDuration = LightDict.Values.First().First().TickCount * 2;
            foreach (List<TrafficLight> lights in LightDict.Values)
                foreach (TrafficLight light in lights)
                    light.Duration = lightDuration;
        }

        /// <summary>
        /// CAlls the update method of all traffic lights in the group
        /// </summary>
        /// <param name="stateChanged">Reference to the value which tells if at least one traffic light has changed his state</param>
        public void UpdateTrafficLights(ref bool stateChanged)
        {
            foreach (List<TrafficLight> lights in LightDict.Values)
                foreach (TrafficLight light in lights)
                    light.update(ref stateChanged);
        }
    }
}
