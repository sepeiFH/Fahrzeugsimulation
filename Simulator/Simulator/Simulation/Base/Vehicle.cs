using Simulator.VehicleAgents;
using System;

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
        public VehicleMovementAgent driver;
        public double Width { get; set; }
        public double Length { get; set; }
        public VehicleList vehicleType { get; set; }

        /// <summary>
        /// Enum for the possible vehicle types, values equals the GID of the values
        /// </summary>
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
        public Vehicle(Random rand, int gid, double rawVelocity, double rawAccerleration, double rawDeceleration) : base()
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

            driver.MaxVelocity = (rawVelocity * 10) / Program.settings.Takt;
            driver.MaxAcceleration = (rawAccerleration * 10) / Program.settings.Takt;
            driver.MaxDeceleration = (rawDeceleration * 10) / Program.settings.Takt;
        }

        public override void update()
        {
            driver.act(this);
        }
    }
}
