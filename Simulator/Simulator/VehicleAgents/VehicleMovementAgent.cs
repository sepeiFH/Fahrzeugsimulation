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
        public double AllowedVelocity { get; set; }
        public double ActAcceleration { get; set; }
        private bool debug = false;
        Dictionary<side, int> mapFieldsInDirection;
        private int degreesToRotate = 0;
        private CrossingDirection nextMove = CrossingDirection.None;

        public int vehicleMapPositionX { get; set; }
        public int vehicleMapPositionY { get; set; }
        private int count = 0;
        private Random rand;
        private bool rotationAllowed = false;

        private character Character { get; set; }

        //private Dictionary<side, List<double>> visibleMap;

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
            this.rand = rand;
            Array chars = Enum.GetValues(typeof(character));
            Character = (character)chars.GetValue(rand.Next(chars.Length));
        }
        #endregion


        internal virtual void act(Vehicle vehicle)
        {
            int actPosInMapListX = -1;
            int actPosInMapListY = -1;
            int actOffsetX = -1;
            int actOffsetY = -1;
            // load street map
            List<List<StreetBlock>> vehiclesStreetMap = loadStreetMap(vehicle.X, vehicle.Y, vehicle.Rotation, ref actPosInMapListX, ref actPosInMapListY, ref actOffsetX, ref actOffsetY);
            // load list of dynamic objects
            SortedDictionary<double, List<DynamicBlock>> dynamicListWithDistance = loadDynamicBlockList(vehicle);
            //side destinationSide = routeDecision();
            moveVehicle(vehicle, vehiclesStreetMap, dynamicListWithDistance, actPosInMapListX, actPosInMapListY, actOffsetX, actOffsetY);
        }

        #region vehicle interaction
        public virtual void moveVehicle(Vehicle vehicle, List<List<StreetBlock>> vehiclesStreetMap, SortedDictionary<double, List<DynamicBlock>> dynamicList, int actPosInMapListX, int actPosInMapListY, int actOffsetX, int actOffsetY)
        {
            double newX = vehicle.X;
            double newY = vehicle.Y;

            CrossingBlock crossingAhead = null;
            StreetBlock lastBlock = null;
            StreetBlock actualBlock = null;
            StreetBlock aheadBlock = null;
            #region react on street map
            // find vehicle in his map
            // If vehicle isn't on his own map, drive foreward to get into the simulationMap
            if (actPosInMapListX > 0)
            {
                // calculate Blocks ahead and behind the vehicle
                // only get last block if car drives without degrees to rotate
                if (degreesToRotate == 0 && actPosInMapListY >= 0 && actPosInMapListX - 1 >= 0 && vehiclesStreetMap[actPosInMapListY].Count - 1 >= actPosInMapListX)
                    lastBlock = vehiclesStreetMap[actPosInMapListY][actPosInMapListX - 1];
                if (actPosInMapListY >= 0 && vehiclesStreetMap[actPosInMapListY].Count - 1 >= actPosInMapListX)
                    actualBlock = vehiclesStreetMap[actPosInMapListY][actPosInMapListX];
                if (actPosInMapListY >= 0 && vehiclesStreetMap[actPosInMapListY].Count - 1 >= actPosInMapListX + 1)
                    aheadBlock = vehiclesStreetMap[actPosInMapListY][actPosInMapListX + 1];
                if (actPosInMapListY >= 0 && vehiclesStreetMap[actPosInMapListY].Count - 1 >= actPosInMapListX + 3 && vehiclesStreetMap[actPosInMapListY][actPosInMapListX + 3] != null && vehiclesStreetMap[actPosInMapListY][actPosInMapListX + 3].GetType() == typeof(CrossingBlock))
                    crossingAhead = (CrossingBlock)vehiclesStreetMap[actPosInMapListY][actPosInMapListX + 3];
            }
            // if there is a crossing ahead and there is no next Move, set nextMove and degrees to rotate
            if (crossingAhead != null && nextMove == CrossingDirection.None)
            {
                nextMove = crossingAhead.PossibleCrosDirs[rand.Next(crossingAhead.PossibleCrosDirs.Count - 1)];
                if (nextMove == CrossingDirection.Left)
                    degreesToRotate = -90;
                else if (nextMove == CrossingDirection.Right)
                    degreesToRotate = 90;
            }
            if (aheadBlock != null && actualBlock != null)
            {
                if (actualBlock.GetType() == typeof(CrossingBlock) && aheadBlock.Direction == StreetDirection.Crossing && nextMove == CrossingDirection.Right && ((actOffsetX < 15 && (vehicle.Rotation == 0 || vehicle.Rotation == 180)) || (actOffsetY > 15 && (vehicle.Rotation == 90 || vehicle.Rotation == 270))))
                {
                    rotationAllowed = true;
                    //calcNewAccerleration(false, true, -1d);
                }
                if (actualBlock.Direction == StreetDirection.Crossing && actualBlock.Direction == StreetDirection.Crossing && nextMove == CrossingDirection.Left && ((actOffsetX < 15 && (vehicle.Rotation == 0 || vehicle.Rotation == 180)) || (actOffsetY > 15 && (vehicle.Rotation == 90 || vehicle.Rotation == 270))))
                {
                    rotationAllowed = true;
                    //calcNewAccerleration(false, true, -1d);
                }
                if (lastBlock != null && lastBlock.Direction == StreetDirection.Crossing && actualBlock.Direction != StreetDirection.Crossing && nextMove == CrossingDirection.Straight)
                {
                    nextMove = CrossingDirection.None;
                }

                if (degreesToRotate > 0 && actualBlock.GetType() == typeof(CrossingBlock) && aheadBlock.Direction == StreetDirection.Crossing)
                {
                    //MaxVelocity /= 6;
                    /*
                    vehicle.Rotation += 15;
                    degreesToRotate -= 15;

                    if (vehicle.Rotation == 360)
                        vehicle.Rotation = 0;*/
                }
                else if (degreesToRotate < 0 && actualBlock.Direction == StreetDirection.Crossing && aheadBlock.Direction == StreetDirection.Crossing && actOffsetX >= 25)
                {
                    //MaxVelocity /= 6;
                    /*
                     if (vehicle.Rotation == 0)
                        vehicle.Rotation = 360;

                    vehicle.Rotation -= 15;
                    degreesToRotate += 15;*/
                }
                //calcnewPosition(vehicle);
                //calcNewVelocity();
                //calcNewAccerleration(true, -1d);
            }

            if (degreesToRotate > 0 && degreesToRotate <= 90 && rotationAllowed)
            {
                if (vehicle.Rotation == 360)
                    vehicle.Rotation = 0;

                vehicle.Rotation += 15;
                degreesToRotate -= 15;

                if (degreesToRotate == 0)
                {
                    nextMove = CrossingDirection.None;
                    rotationAllowed = false;
                    //MaxVelocity *= 6;
                }
            }
            else if (degreesToRotate < 0 && degreesToRotate >= -90 && rotationAllowed)
            {
                if (vehicle.Rotation == 0)
                    vehicle.Rotation = 360;

                vehicle.Rotation -= 15;
                degreesToRotate += 15;
                if (degreesToRotate == 0)
                {
                    nextMove = CrossingDirection.None;
                    rotationAllowed = false;
                    //MaxVelocity *= 6;
                }
            }

            double roundSpeed = ActVelocity + ActAcceleration;
            if (vehicle.IsBroken)
                return;
            /*if (roundSpeed < MaxVelocity)
            {
                ActVelocity = roundSpeed;
                ActAcceleration = MaxAcceleration;
            }
            else
            {
                ActVelocity = MaxVelocity;
                ActAcceleration = 0;
            }*/

            #endregion

            #region react on dynamic objects
            //List<double> nearestDynamicObject = dynamicList.Keys;
            bool reactedOnTrafficSign = false;

            foreach (var dynamicBlockList in dynamicList)
            {
                foreach (DynamicBlock dynamicBlock in dynamicBlockList.Value)
                {
                    if (dynamicBlock.GetType().Equals(typeof(Vehicle)))
                    {
                        Vehicle tempVehicle = (Vehicle)dynamicBlock;
                        // Fahrzeug fährt in gleiche Richtung, ist fahrfähig, hat geringere Geschwindigkeit und Distanz ist im Bremsbereich
                        if (vehicle.Rotation.Equals(tempVehicle.Rotation) && !vehicle.IsBroken && vehicle.driver.ActVelocity > tempVehicle.driver.ActVelocity && isBlockInReactionSpan(calcDistanceForSameDir(vehicle, tempVehicle), vehicle, tempVehicle))
                        {
                            AllowedVelocity = vehicle.driver.ActVelocity;
                        }
                    }
                    else if (dynamicBlock.GetType().Equals(typeof(TrafficLight)) && !Simulator.Simulation.Simulator.EmergencyModeActive && (vehicle.Rotation + 180) % 360 == dynamicBlock.Rotation && isDynamicBlockAhead(vehicle, dynamicBlock) && isBlockInReactionSpan(dynamicBlockList.Key, vehicle)
                        )
                    {
                        //reactedOnTrafficSign = true;
                        if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Yellow))
                        {
                            AllowedVelocity = MaxVelocity;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Red))
                        {
                            AllowedVelocity = 0;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Green))
                        {
                            AllowedVelocity = MaxVelocity;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.YellowRed) && Character.Equals(character.aggresive))
                        {
                            AllowedVelocity = MaxVelocity;
                        }
                    }
                }
            }
            #endregion

            calcnewPosition(vehicle);
            calcNewVelocity();
        }

        private double calcDistanceForSameDir(Vehicle vehicle, Vehicle tempVehicle)
        {
            return 0.0;
        }

        private bool isBlockInReactionSpan(double distance, Vehicle vehicle)
        {

            if (distance < vehicle.Length && distance - ((ActVelocity * ActVelocity) / (2 * MaxDeceleration)) >= 0 || ActVelocity == 0)
                return true;

            return false;
        }

        private bool isBlockInReactionSpan(double distance, Vehicle ownVehicle, Vehicle otherVehicle)
        {
            if (distance < ownVehicle.Length && distance - ((ActVelocity * ActVelocity) / (2 * MaxDeceleration)) >= 0 || ActVelocity == 0)
                return true;

            return false;
        }

        private bool isDynamicBlockAhead(Vehicle vehicle, DynamicBlock dynamicBlock)
        {
            bool isAhead = false;
            // mathematical way:

            // tryout way
            // 0 degree
            if ((vehicle.Rotation >= 0 && vehicle.Rotation < 45 || vehicle.Rotation >= 315 && vehicle.Rotation < 360) && vehicle.X > dynamicBlock.X)
                isAhead = true;
            // around 90 degree
            else if ((vehicle.Rotation >= 45 && vehicle.Rotation < 135) && vehicle.Y < dynamicBlock.Y)
                isAhead = true;
            // around 180 degree
            else if ((vehicle.Rotation >= 135 && vehicle.Rotation < 225) && vehicle.X < dynamicBlock.X)
                isAhead = true;
            // 270 degree
            else if (vehicle.Y > dynamicBlock.Y)
                isAhead = true;

            return isAhead;
        }

        private void calcnewPosition(Vehicle vehicle)
        {
            double newX = 0;
            double newY = 0;

            double yy = vehicle.Y + ActVelocity;

            double angle = (vehicle.Rotation + 90) % 360;

            double radiants = angle * (Math.PI / 180.0d);
            newX = (Math.Cos(radiants) * (double)(vehicle.X - vehicle.X) - Math.Sin(radiants) * (double)(yy - vehicle.Y) + vehicle.X);
            newY = (Math.Sin(radiants) * (double)(vehicle.X - vehicle.X) + Math.Cos(radiants) * (double)(yy - vehicle.Y) + vehicle.Y);

            vehicle.X = newX;
            vehicle.Y = newY;
        }

        private void calcNewVelocity()
        {
            double newVelocity = 0;
            if (MaxVelocity < AllowedVelocity)
            {
                if (ActVelocity == MaxVelocity)
                {
                    newVelocity = ActVelocity;
                    calcNewAccerleration(false, false, -1d);
                }
                else if (ActVelocity > MaxVelocity)
                {
                    newVelocity = ActVelocity + ActAcceleration;
                    calcNewAccerleration(false, true, -1d);
                }
                else if (ActVelocity + ActAcceleration <= MaxVelocity)
                {
                    newVelocity = ActVelocity + ActAcceleration;
                    calcNewAccerleration(true, false, -1d);
                }
                else if (ActVelocity + ActAcceleration > MaxVelocity)
                {
                    newVelocity = MaxVelocity;
                    calcNewAccerleration(false, false, -1d);
                }
            }
            else
            {
                if (ActVelocity == AllowedVelocity)
                {
                    newVelocity = ActVelocity;
                    calcNewAccerleration(false, false, -1d);
                }
                else if (ActVelocity > AllowedVelocity)
                {
                    newVelocity = ActVelocity + ActAcceleration;
                    calcNewAccerleration(false, true, -1d);
                }
                else if (ActVelocity + ActAcceleration <= AllowedVelocity)
                {
                    newVelocity = ActVelocity + ActAcceleration;
                    calcNewAccerleration(true, false, -1d);
                }
                else if (ActVelocity + ActAcceleration > AllowedVelocity)
                {
                    newVelocity = AllowedVelocity;
                    calcNewAccerleration(false, false, -1d);
                }
            }

            ActVelocity = newVelocity;
        }

        private void calcNewAccerleration(bool accelerate, bool decelerate, Double fieldsToBreak)
        {
            double newAcc = 0;
            if (accelerate)
            {
                if (MaxVelocity > ActVelocity && ActAcceleration < MaxAcceleration && Character == character.aggresive)
                    newAcc = ActAcceleration + (MaxAcceleration - ActAcceleration);
                else if (MaxVelocity > ActVelocity && ActAcceleration < MaxAcceleration)
                    if ((MaxAcceleration - ActAcceleration) - 1 * Program.settings.Takt >= 0)
                        newAcc = ActAcceleration + (MaxAcceleration - ActAcceleration) - 1 * Program.settings.Takt;
                    else
                        newAcc = ActAcceleration + (MaxAcceleration - ActAcceleration);
                else if (MaxVelocity > ActVelocity)
                    newAcc = ActAcceleration;
            }
            else if (decelerate)
            {
                if (ActVelocity > 0 && ActAcceleration > MaxDeceleration * -1 && Character == character.aggresive)
                    newAcc = ActAcceleration - (MaxDeceleration + ActAcceleration);
                else if (ActVelocity > 0 && ActAcceleration > MaxDeceleration)
                    if (MaxDeceleration + ActAcceleration - 1 * Program.settings.Takt >= 0)
                        newAcc = ActAcceleration - (MaxDeceleration + ActAcceleration) - 1 * Program.settings.Takt;
                    else
                        newAcc = ActAcceleration - (MaxDeceleration + ActAcceleration);
                else if (ActVelocity > 0)
                    newAcc = ActAcceleration;
                else
                    newAcc = 0;
            }

            ActAcceleration = newAcc;
        }
        #endregion
        #region simulation interaction

        protected virtual void initFieldDirection()
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
        }

        public virtual SortedDictionary<double, List<DynamicBlock>> loadDynamicBlockList(Vehicle vehicle)
        {
            initFieldDirection();
            Simulation.Simulator simu = Simulation.Simulator.Instance;

            SortedDictionary<double, List<DynamicBlock>> dynamicListWithDistance = simu.allDynamicObjectsInRange(vehicle.X, vehicle.Y, vehicle.Rotation, mapFieldsInDirection);
            return dynamicListWithDistance;
        }

        public virtual List<List<StreetBlock>> loadStreetMap(double X, double Y, double rotation, ref int actPosX, ref int actPosY, ref int actOffSetX, ref int actOffsetY)
        {
            initFieldDirection();

            Simulation.Simulator simu = Simulation.Simulator.Instance;

            List<List<StreetBlock>> mapList = simu.getMapInfo(X, Y, rotation, mapFieldsInDirection, ref actPosX, ref actPosY, ref actOffSetX, ref actOffsetY);

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
