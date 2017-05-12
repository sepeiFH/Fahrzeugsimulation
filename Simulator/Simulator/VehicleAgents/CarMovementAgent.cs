using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.VehicleAgents
{
    class CarMovementAgent : VehicleMovementAgent
    {
        public CarMovementAgent(Random rand) : base (rand)
        {
        }

        /*public override void loadMap(double X, double Y)
        {
            throw new NotImplementedException();
        }*/
    }
}
