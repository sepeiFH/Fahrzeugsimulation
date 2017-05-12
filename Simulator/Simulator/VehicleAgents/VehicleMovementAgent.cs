using Simulator.Simulation.Base;
using System;
using System.Collections.Generic;

namespace Simulator.VehicleAgents
{
    public abstract class VehicleMovementAgent
    {
        #region Fields
        #region private Fields
        public double MaxVelocity { get; set; }
        public double MaxAcceleration { get; set; }
        public double MaxDeceleration { get; set; }
        public double ActVelocity { get; set; }
        public double ActAcceleration { get; set; }
        private bool debug = false;
        Dictionary<side, int> mapFieldsInDirection;

        public int vehicleMapPositionX { get; set; }
        public int vehicleMapPositionY { get; set; }
        private int count = 0;

        private character Character { get; set; }

        private Dictionary<side, List<double>> visibleMap;

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


        internal virtual void act(Vehicle vehicle)
        {
            int actPosInMapListX = -1;
            int actPosInMapListY = -1;
            List<List<StreetBlock>> vehiclesStreetMap = loadMap(vehicle.X, vehicle.Y, vehicle.Rotation, ref actPosInMapListX, ref actPosInMapListY);
            //side destinationSide = routeDecision();
            moveVehicle(vehicle, vehiclesStreetMap, actPosInMapListX, actPosInMapListY);
        }

        #region vehicle interaction
        public virtual void moveVehicle(Vehicle vehicle, List<List<StreetBlock>> vehiclesStreetMap, int actPosInMapListX, int actPosInMapListY)
        {
            double newX = vehicle.X;
            double newY = vehicle.Y;

            StreetBlock actBlock = null;
            StreetBlock frontOfVehicle = null;
            // find vehicle in his map

            // If vehicle isn't on his own map, driver foreward to get into the simulationMap
            if (actPosInMapListY >= 0 && vehiclesStreetMap[actPosInMapListY].Count - 1 >= actPosInMapListX + 2)
                frontOfVehicle = vehiclesStreetMap[actPosInMapListY][actPosInMapListX + 2];
            /* if (actPosInMapListX != -1 && frontOfVehicle != null)
             {
                 if (frontOfVehicle.Direction == StreetDirection.Crossing)
                 {
                 }
             }
             else
             {*/
            double roundSpeed = ActVelocity + ActAcceleration;
            if (vehicle.IsBroken)
                return;
            if (roundSpeed < MaxVelocity)
            {
                ActVelocity = roundSpeed;
                ActAcceleration = MaxAcceleration;
            }
            else
            {
                ActVelocity = MaxVelocity;
                ActAcceleration = 0;
            }

            double yy = vehicle.Y + ActVelocity;
            double angle = (vehicle.Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            newX = (Math.Cos(radiants) * (double)(vehicle.X - vehicle.X) - Math.Sin(radiants) * (double)(yy - vehicle.Y) + vehicle.X);
            newY = (Math.Sin(radiants) * (double)(vehicle.X - vehicle.X) + Math.Cos(radiants) * (double)(yy - vehicle.Y) + vehicle.Y);
            //}
            vehicle.X = newX;
            vehicle.Y = newY;
        }

        private void oldMove()
        {
            /*
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
            */
        }

        private double calcNewAccerleration(double actAcc, double maxAcc, bool isMaxVelocity)
        {
            double newAcc = 0;
            if (!isMaxVelocity && actAcc < maxAcc && Character == character.aggresive)
                newAcc = actAcc + maxAcc;
            else if (!isMaxVelocity && actAcc < maxAcc)
                newAcc = actAcc + maxAcc - 1;
            else if (!isMaxVelocity)
                newAcc = actAcc;

            return newAcc;
        }
        #endregion
        #region simulation interaction

        public virtual List<List<StreetBlock>> loadMap(double X, double Y, double rotation, ref int actPosX, ref int actPosY)
        {
            if (mapFieldsInDirection == null)
            {
                mapFieldsInDirection = new Dictionary<side, int>();

                int faktor;
                switch (Character)
                {
                    case character.aggresive:
                        faktor = 1;
                        break;
                    case character.peaceful:
                        faktor = 2;
                        break;
                    default:
                        faktor = 1;
                        break;
                }
                mapFieldsInDirection.Add(side.foreward, (int)Math.Ceiling(faktor * (MaxVelocity)));
                mapFieldsInDirection.Add(side.backward, (int)Math.Ceiling(MaxVelocity));
                mapFieldsInDirection.Add(side.left, (int)Math.Ceiling(MaxVelocity));
                mapFieldsInDirection.Add(side.right, (int)Math.Ceiling(MaxVelocity));
            }

            Simulation.Simulator simu = Simulation.Simulator.Instance;

            List<List<StreetBlock>> mapList = simu.getMapInfo(X, Y, rotation, mapFieldsInDirection, ref actPosX, ref actPosY);

            // virtualize for debug causes (set debug = true if you want to see one vehicles map)
            if (debug)
            {
                Console.Clear();
                foreach (List<StreetBlock> row in mapList)
                {
                    foreach (StreetBlock entry in row)
                    {
                        if (entry == null)
                            Console.Write("0");
                        else
                            Console.Write(entry.GID);
                    }
                    Console.Write("\n");
                }
            }

            return mapList;
        }

        #endregion
    }
}
