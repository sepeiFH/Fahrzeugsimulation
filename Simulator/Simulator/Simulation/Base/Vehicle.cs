using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.Simulation.Base
{
    public class Vehicle : DynamicBlock
    {
        public override double X { get; set; }
        public override double Y { get; set; }
        public override double Rotation { get; set; }
        public override int GID { get; set; }

        public enum VehicleList
        {
            LKW1 = 37,
            Car1 = 38,
        }
        public Vehicle()
        {
        }

        private Dictionary<VehicleList, int> vehicleLengths = new Dictionary<VehicleList, int>() { { VehicleList.LKW1, 60 }, { VehicleList.Car1, 32 } };
        private int count;
        public override void update()
        {/*
            if (X < -100)
                X = 2000;
            else if (X > 2000)
                X = -100;

            if (Y < -100)
                Y = 1000;
            if (Y > 1000)
                Y = -100;

            MoveVehicle(5);
            if (count++ % 2 == 0)
            {
                this.Rotation += 0.25;
                count = 2;
            }*/

        }

        //Clockwise Rotation: Left Startpoint 90° Top 180 Right 270 Down
        public void MoveVehicle(double doublePixels)
        {/*
            double yy = Y + doublePixels;
            double angle = (Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            double newX = (Math.Cos(radiants) * (double)(X - X) - Math.Sin(radiants) * (double)(yy - Y) + X);
            double newY = (Math.Sin(radiants) * (double)(X - X) + Math.Cos(radiants) * (double)(yy - Y) + Y);

            X = newX;
            Y = newY;*/
        }
    }
}
