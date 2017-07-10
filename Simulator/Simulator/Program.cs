using Simulator.Simulation.Base;
using Simulator.Simulation.Utilities;
using Simulator.Simulation.WCFInterfaces;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;
using static Simulator.Simulation.Base.Vehicle;

namespace Simulator
{
    /// <summary>
    /// Main Class of the Simulator-Application
    /// </summary>
    class Program
    {
        public static SimulationConfig settings = (dynamic)ConfigurationManager.GetSection("simulationConfig");

        /// <summary>
        /// Main-Method of the simulator-application. Opens the Host for the WCF-services and initialize and starts the simulatior.
        /// </summary>
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri(settings.BaseUrl);

            Simulation.Simulator simu = Simulation.Simulator.Instance;
            simu.init();
            simu.GetSimulatorThread().Start();
            SimulatorService.mapString = simu.mapString;
            SimulatorService.simu = simu;

            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(SimulatorService), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("The service is ready at {0}", baseAddress);
                //Console.WriteLine("Press <Enter> to stop the service.");
                Random rand = new Random();
                int id = 0;
                while (true)
                {
                    Console.WriteLine("Add Vehicle: GID, maxVelocity, X, Y, Rotation");
                    string input = Console.ReadLine();
                    string[] inputList = input.Split(';');
                    int vehicleGID = int.Parse(inputList[0]);
                    double maxVelocity = double.Parse(inputList[1]);
                    double vehicleX = double.Parse(inputList[2]);
                    double vehicleY = double.Parse(inputList[3]);
                    double vehicleRotation = double.Parse(inputList[4]);
                    createVehicle(id, rand, vehicleGID, maxVelocity, vehicleX, vehicleY, vehicleRotation);
                    id++;
                }

                // Close the ServiceHost.
                host.Close();
            }
        }

        // Test-Method
        private static void createVehicle(int id, Random rand, int vehicleGID, double maxVelocity, double vehicleX, double vehicleY, double vehicleRotation)
        {
            ConfigVehicle tempVehicle = Program.settings.Vehicles.Find(x => x.GID == vehicleGID);
            Vehicle vehicle = new Vehicle(rand, vehicleGID, maxVelocity, tempVehicle.MaxAcceleration, tempVehicle.MaxDeceleration)
            {
                ID = id,
                Rotation = vehicleRotation,
                X = vehicleX,
                Y = vehicleY,
                Width = tempVehicle.Width,
                Length = tempVehicle.Length,
                vehicleType = tempVehicle.Type.Contains("KFZ1") ? VehicleList.Car1 : (tempVehicle.Type.Contains("KFZ2") ? VehicleList.Car2 : (tempVehicle.Type.Contains("LKW1") ? VehicleList.Truck1 : (tempVehicle.Type.Contains("LKW2") ? VehicleList.Truck2 : VehicleList.Motorcycle)))
            };
            Simulator.Simulation.Simulator.vehiclesToAdd.Add(vehicle);
        }
    }
}
