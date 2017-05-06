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
        }
        public Vehicle(Random rand,int gid)
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
        public override void update()
        {
            driver.moveVehicle(this);
        }
    }
}
