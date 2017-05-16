using System;
using Traffic_control.Trafficcontrol;

namespace Traffic_control
{
    class Program
    {
        static void Main(string[] args)
        {
            TrafficAgent agent = new TrafficAgent();
            agent.StartTrafficAgent();
            Console.ReadLine();
        }
    }
}
