using Simulator.Simulation.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator.VehicleAgents
{
    public abstract class VehicleMovementAgent
    {
        #region Fields
        #region private Fields
        private int Fieldsforward;
        private int FieldsBackwards;
        private int FieldsLeft;
        private int FieldsRigth;
        private int count = 0;
        private character Character { get; set; }

        private List<List<double>> visibleMap;

        #region public Fields 
        public enum side
        {
            foreward,
            backward,
            left,
            right
        }

        public enum character
        {
            peaceful,
            aggresive
        }
        #endregion
        #endregion
        #endregion

        #region Constructor
        public VehicleMovementAgent(Random rand)
        {

            Array chars = Enum.GetValues(typeof(character));
            Character = (character)chars.GetValue(rand.Next(chars.Length));
        }
        #endregion

        public virtual void moveVehicle(Vehicle vehicle)
        {
            if (vehicle.IsBroken)
                return;
            double doublePixels = 5;
            if (count++ % 2 == 0)
            {
                vehicle.Rotation += 0.25;
                count = 2;
            }
            
           double yy = vehicle.Y + doublePixels;
            double angle = (vehicle.Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            double newX = (Math.Cos(radiants) * (double)(vehicle.X - vehicle.X) - Math.Sin(radiants) * (double)(yy - vehicle.Y) + vehicle.X);
            double newY = (Math.Sin(radiants) * (double)(vehicle.X - vehicle.X) + Math.Cos(radiants) * (double)(yy - vehicle.Y) + vehicle.Y);

            vehicle.X = newX;
            vehicle.Y = newY;
        }
    }
}
