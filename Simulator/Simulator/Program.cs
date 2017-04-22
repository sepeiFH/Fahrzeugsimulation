using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IO;
using Simulator.Simulation;
using Simulator.Simulation.Base;
using Simulator.Simulation.WCFInterfaces;
using TiledSharp;

namespace Simulator
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:1667/Simulator");

            Simulation.Simulator simu = new Simulation.Simulator();
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
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}
