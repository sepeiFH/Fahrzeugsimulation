using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Utilities
{
    class Utils
    {
        /// <summary>
        /// Get the distance squared between two points
        /// </summary>
        /// <param name="point1">point one</param>
        /// <param name="point2">point two</param>
        /// <returns>distance</returns>
        public static double GetDistanceSqr(double x, double y, double x1, double y1)
        {
            double deltaX = x - x1;
            double deltaY = y - y1;

            return deltaX * deltaX + (deltaY * deltaY);
        }

        public static double GetDistance(double x, double y, double x1, double y1)
        {
            return Math.Round(Math.Sqrt(GetDistanceSqr(x, y, x1, y1)));
        }
    }
}
