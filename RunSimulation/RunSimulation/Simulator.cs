using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RunSimulation
{

    class Simulator
    {
        public Simulator()
        {

        }

        public void RunSimulation()
        {
            List<string> Apps = new List<string> { @"..\..\..\..\Simulator\Simulator\bin\Debug\Simulator.exe", @"..\..\..\..\Traffic control\Traffic control\bin\Debug\Traffic control.exe", @"..\..\..\..\FhMapDrawing\FhMapDrawing\bin\Windows\x86\Debug\FhMapDrawing.exe" };

            foreach (string app in Apps)
            {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = app;
                Process.Start(start);
                int t = app.LastIndexOf(@"\") + 1;
                string name = app.Substring(t, app.Length - t - 4);
                Console.WriteLine("Anwendung " + name + " wurde gestartet");
                Thread.Sleep(1000);
            }

            Console.WriteLine("Fahrzeugsimulation wurde gestartet");
            Console.ReadLine();
        }
    }
}
