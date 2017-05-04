using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Traffic_control.Trafficcontrol
{
    public class TrafficLightGroup
    {
        public int ID { get; set; }

        public Dictionary<Position,List<TrafficLight>> LightDict { get; set; }

        public enum Position
        {
            Top,
            Bottom,
            Right,
            Left
        }

        public void setOffset()
        {
            int lightDuration = LightDict.Values.First().First().TickCount;
            // 4 Ampeln
            if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
            {
                foreach (TrafficLight light in LightDict[Position.Right])
                    light.Offset = lightDuration;
                foreach (TrafficLight light in LightDict[Position.Left])
                    light.Offset = lightDuration;
            }
            // 3 Ampeln, Top fehlt
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Bottom])
                    light.Offset = lightDuration;
            // 3 Ampeln, Bottom fehlt
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top))
                foreach (TrafficLight light in LightDict[Position.Top])
                    light.Offset = lightDuration;
            // 3 Ampeln, Left fehlt
            else if (LightDict.ContainsKey(Position.Right) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Right])
                    light.Offset = lightDuration;
            // 3 Ampeln, Right fehlt
            else if (LightDict.ContainsKey(Position.Left) && LightDict.ContainsKey(Position.Top) && LightDict.ContainsKey(Position.Bottom))
                foreach (TrafficLight light in LightDict[Position.Left])
                    light.Offset = lightDuration;
        }

        public void SetDuration()
        {
            int lightDuration = LightDict.Values.First().First().TickCount*2;
            foreach (List<TrafficLight> lights in LightDict.Values)
                foreach (TrafficLight light in lights)
                    light.Duration = lightDuration;
        }

        public void UpdateTrafficLights(ref bool stateChanged)
        {
            foreach (List<TrafficLight> lights in LightDict.Values)
                foreach (TrafficLight light in lights)
                    light.update(ref stateChanged);
        }
    }
}
