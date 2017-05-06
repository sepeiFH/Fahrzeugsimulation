using Simulator.VehicleAgents;
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
        public override int ID { get; set; }
        public override bool IsBroken { get; set; }
        private VehicleMovementAgent driver;

        public enum VehicleList
        {
            Car1 = 1004,
            Car2 = 1001,
            Truck1 = 1000,
            Truck2 = 1002,
            Motorcycle = 1003
        }
        public Vehicle()
        {
            IsBroken = false;
        }
        public Vehicle(Random rand,int gid) : base()
        {
            this.GID = gid;
            switch (GID)
            {
                case (int)VehicleList.Car1:
                    driver = new CarMovementAgent(rand);
                    break;
                case (int)VehicleList.Car2:
                    driver = new CarMovementAgent(rand);
                    break;
                case (int)VehicleList.Truck1:
                    driver = new TruckMovementAgent(rand);
                    break;
                case (int)VehicleList.Truck2:
                    driver = new TruckMovementAgent(rand);
                    break;
                case (int)VehicleList.Motorcycle:
                    driver = new MotorCycleMovementAgent(rand);
                    break;
            }
        }

        private Dictionary<VehicleList, int> vehicleLengths = new Dictionary<VehicleList, int>() { { VehicleList.Truck1, 60 }, { VehicleList.Car1, 32 } };
        private int count;
        public override void update()
        {
            driver.moveVehicle(this);
            /*MoveVehicle(5);
            if (count++ % 2 == 0)
            {
                this.Rotation += 0.25;
                count = 2;
            }*/

        }

        //Clockwise Rotation: Left Startpoint 90° Top 180 Right 270 Down
        public void MoveVehicle(double doublePixels)
        {
            double yy = Y + doublePixels;
            double angle = (Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            double newX = (Math.Cos(radiants) * (double)(X - X) - Math.Sin(radiants) * (double)(yy - Y) + X);
            double newY = (Math.Sin(radiants) * (double)(X - X) + Math.Cos(radiants) * (double)(yy - Y) + Y);

            X = newX;
            Y = newY;
        }
    }
}
