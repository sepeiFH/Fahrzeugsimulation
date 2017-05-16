using Simulator.Simulation.Base;
using Simulator.Simulation.Utilities;
using Simulator.Simulation.WCFInterfaces;
using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Simulator
{
    class Program
    {
        public static SimulationConfig settings = (dynamic)ConfigurationManager.GetSection("simulationConfig");
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri(settings.BaseUrl);

            Simulation.Simulator simu = Simulation.Simulator.Instance;
            simu.init(settings);
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
                Console.WriteLine("Press <Enter> to stop the service.");
                Random rand = new Random();
                while (true)
                {
                    string input = Console.ReadLine();
                    string[] inputList = input.Split(';');
                    int vehicleGID = int.Parse(inputList[0]);
                    double maxVelocity = double.Parse(inputList[1]);
                    double vehicleX = double.Parse(inputList[2]);
                    double vehicleY = double.Parse(inputList[3]);
                    double vehicleRotation = double.Parse(inputList[4]);
                    createVehicle(rand, vehicleGID, maxVelocity, vehicleX, vehicleY, vehicleRotation);
                }

                // Close the ServiceHost.
                host.Close();
            }
        }
        private static void createVehicle(Random rand, int vehicleGID, double maxVelocity, double vehicleX, double vehicleY, double vehicleRotation)
        {
            ConfigVehicle tempVehicle = Program.settings.Vehicles.Find(x => x.GID == vehicleGID);
            Vehicle vehicle = new Vehicle(rand, vehicleGID, maxVelocity, tempVehicle.MaxAcceleration, tempVehicle.MaxDeceleration)
            {
                Rotation = vehicleRotation,
                X = vehicleX,
                Y = vehicleY,
                Height = tempVehicle.Height,
                Width = tempVehicle.Width
            };
            Simulator.Simulation.Simulator.vehiclesToAdd.Add(vehicle);
        }
    }
}
