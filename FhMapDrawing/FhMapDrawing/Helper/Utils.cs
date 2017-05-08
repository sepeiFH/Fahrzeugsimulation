using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace FhMapDrawing.Helper
{
    class Utils
    {
        /// <summary>
        /// Get the distance squared between two points
        /// </summary>
        /// <param name="point1">point one</param>
        /// <param name="point2">point two</param>
        /// <returns>distance</returns>
        public static double GetDistanceSqr(Point point1, Point point2)
        {
            int deltaX = point1.X - point2.X;
            int deltaY = point1.Y - point2.Y;

            return (double)(deltaX * deltaX) + (deltaY * deltaY);
        }


        /// <summary>
        /// Get the distance between two points
        /// </summary>
        /// <param name="point1">point one</param>
        /// <param name="point2">point two</param>
        /// <returns>distance</returns>
        public static double GetDistance(Point point1, Point point2)
        {
            return (double)Math.Round(Math.Sqrt(GetDistanceSqr(point1, point2)));
        }
    }
}
