using Simulator.Simulation.Base;
using System;
using System.Collections.Generic;

namespace Simulator.VehicleAgents
{
    public abstract class VehicleMovementAgent
    {
        #region Fields
        #region private Fields
        private bool debug = false;
        Dictionary<side, int> mapFieldsInDirection;
        private int degreesToRotate = 0;
        private CrossingDirection nextMove = CrossingDirection.None;
        private bool reactonBrokenVehicleModeOn = false;
        private int count = 0;
        private Random rand;
        private bool rotationAllowed = false;
        private double distanceTodriveBeside = 0;
        private double reactedDistance;
        private int switchLaneCount;
        private double switchLaneStartRotation;
        private character Character { get; set; }

        //private Dictionary<side, List<double>> visibleMap;

        #region public Fields 
        public int vehicleMapPositionY { get; set; }
        public double MaxVelocity { get; set; }
        public double MaxAcceleration { get; set; }
        public double MaxDeceleration { get; set; }
        public double ActVelocity { get; set; }
        public double AllowedVelocity { get; set; }
        public double ActAcceleration { get; set; }
        public int vehicleMapPositionX { get; set; }
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

        /// <summary>
        /// Method contains the loading of the streetmap and the dynamicObject List
        /// </summary>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <returns>void</returns>
        internal virtual void act(Vehicle vehicle)
        {
            int actPosInMapListX = -1;
            int actPosInMapListY = -1;
            int actOffsetX = -1;
            int actOffsetY = -1;
            // load street map
            List<List<StreetBlock>> vehiclesStreetMap = loadStreetMap(vehicle.X, vehicle.Y, vehicle.Rotation, ref actPosInMapListX, ref actPosInMapListY, ref actOffsetX, ref actOffsetY);
            // load list of dynamic objects
            List<DynamicBlock> dynamicList = loadDynamicBlockList(vehicle);

            //side destinationSide = routeDecision();
            moveVehicle(vehicle, vehiclesStreetMap, dynamicList, actPosInMapListX, actPosInMapListY, actOffsetX, actOffsetY);
        }

        #region vehicle interaction
        /* old version, maybe better, save for recovery
        public virtual void moveVehicle(Vehicle vehicle, List<List<StreetBlock>> vehiclesStreetMap, SortedDictionary<double, List<DynamicBlock>> dynamicList, int actPosInMapListX, int actPosInMapListY, int actOffsetX, int actOffsetY)
        {
            reactedDistance = 0;
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
                        if (!vehicle.IsBroken && vehicle.Rotation.Equals(tempVehicle.Rotation) && !reactedOnTrafficSign && vehicle.driver.ActVelocity > tempVehicle.driver.ActVelocity && isBlockInReactionSpan(dynamicBlockList.Key, vehicle, tempVehicle))
                        {
                            AllowedVelocity = vehicle.driver.ActVelocity;
                        }
                        // vehicle in front is broken, so check if second lane is free for driving beside the broken vehicle
                        else if (vehicle.Rotation.Equals(tempVehicle.Rotation) && !reactedOnTrafficSign && isBlockInReactionSpan(dynamicBlockList.Key, vehicle, tempVehicle))
                        {
                            // check if lane is free
                            //if ()
                        }
                    }
                    else if (dynamicBlock.GetType().Equals(typeof(TrafficLight)) && !Simulator.Simulation.Simulator.EmergencyModeActive && (vehicle.Rotation + 180) % 360 == dynamicBlock.Rotation && isDynamicBlockAhead(vehicle, dynamicBlock) && isBlockInReactionSpan(dynamicBlockList.Key, vehicle)
                        )
                    {
                        if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Yellow))
                        {
                            AllowedVelocity = MaxVelocity;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Red))
                        {
                            reactedOnTrafficSign = true;
                            AllowedVelocity = 0;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Green))
                        {
                            reactedOnTrafficSign = false;
                            AllowedVelocity = MaxVelocity;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.YellowRed) && Character.Equals(character.aggresive))
                        {
                            reactedOnTrafficSign = false;
                            AllowedVelocity = MaxVelocity;
                        }
                    }
                }
            }
            #endregion

            calcnewPosition(vehicle);
            calcNewVelocity();
        }*/

        /// <summary>
        /// Method contains the loading of the streetmap and the dynamicObject List
        /// </summary>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <param name="vehiclesStreetMap">map part around the vehicle in the view of the vehicle</param>
        /// <param name="dynamicList">List of all dynamic objects</param>
        /// <param name="actPosInMapListX">x block position of the vehicle</param>
        /// <param name="actPosInMapListY">y block position of the vehicle</param>
        /// <param name="actOffsetX">value which contains the difference between x-value of vehicle and x-value of the actual block</param>
        /// <param name="actOffsetY">value which contains the difference between y-value of vehicle and y-value of the actual block</param>
        /// <returns>void</returns>
        public virtual void moveVehicle(Vehicle vehicle, List<List<StreetBlock>> vehiclesStreetMap, List<DynamicBlock> dynamicList, int actPosInMapListX, int actPosInMapListY, int actOffsetX, int actOffsetY)
        {
            if (vehicle.IsBroken)
                return;

            double newX = vehicle.X;
            double newY = vehicle.Y;

            CrossingBlock crossingAhead = null;
            StreetBlock lastBlock = null;
            StreetBlock actualBlock = null;
            StreetBlock aheadBlock = null;

            #region react on street map
            // find vehicle in his map
            // If vehicle isn't on his own map, drive foreward to get into the simulationMap
            if (!reactonBrokenVehicleModeOn)
            {
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
                    }
                    if (actualBlock.Direction == StreetDirection.Crossing && actualBlock.Direction == StreetDirection.Crossing && nextMove == CrossingDirection.Left && ((actOffsetX < 15 && (vehicle.Rotation == 0 || vehicle.Rotation == 180)) || (actOffsetY > 15 && (vehicle.Rotation == 90 || vehicle.Rotation == 270))))
                    {
                        rotationAllowed = true;
                    }
                    if (lastBlock != null && lastBlock.Direction == StreetDirection.Crossing && actualBlock.Direction != StreetDirection.Crossing && nextMove == CrossingDirection.Straight)
                    {
                        nextMove = CrossingDirection.None;
                    }
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
                    }
                }

                double roundSpeed = ActVelocity + ActAcceleration;

                #endregion

                #region react on dynamic objects

                reactedDistance = 2000;
                bool reactedOnDynObjact = false;
                foreach (DynamicBlock dynamicBlock in dynamicList)
                {
                    double actDistance = calcDistanceForSameDir(vehicle, dynamicBlock);
                    if (dynamicBlock.GetType().Equals(typeof(Vehicle)) && actDistance < reactedDistance)
                    {
                        Vehicle tempVehicle = (Vehicle)dynamicBlock;
                        reactedDistance = actDistance;
                        // vehicle drivs in same direction, can drive/is not broken, and has a smaler speed and his distance is in breaking distance
                        if (!vehicle.IsBroken && vehicle.Rotation.Equals(tempVehicle.Rotation) && this.ActVelocity > tempVehicle.driver.ActVelocity && isBlockInReactionSpan(actDistance, vehicle, tempVehicle))
                        {
                            AllowedVelocity = tempVehicle.driver.ActVelocity;
                            reactedOnDynObjact = true;
                        }
                        // vehicle in front is broken, so check if second lane is free for driving beside the broken vehicle
                        else if (tempVehicle.IsBroken && vehicle.Rotation.Equals(tempVehicle.Rotation) && actDistance < 100 && isBlockInReactionSpan(actDistance, vehicle, tempVehicle))
                        {
                            reactonBrokenVehicleModeOn = true;
                            distanceTodriveBeside = tempVehicle.Length + vehicle.Length + actDistance;
                            switchLaneCount = 0;
                            switchLaneStartRotation = vehicle.Rotation;

                            // TODO: check if lane is free
                            //if ()
                        }
                    }
                    else if (dynamicBlock.GetType().Equals(typeof(TrafficLight)) && !Simulator.Simulation.Simulator.EmergencyModeActive && vehicle.Rotation == ((dynamicBlock.Rotation + 180) % 360) && isDynamicBlockAhead(vehicle, dynamicBlock) && isBlockInReactionSpan(actDistance, vehicle) && actDistance < reactedDistance)
                    {
                        reactedDistance = actDistance;
                        if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Yellow))
                        {
                            AllowedVelocity = MaxVelocity;
                            reactedOnDynObjact = true;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Red))
                        {
                            AllowedVelocity = 0;
                            reactedOnDynObjact = true;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.Green))
                        {
                            AllowedVelocity = MaxVelocity;
                            reactedOnDynObjact = true;
                        }
                        else if (((TrafficLight)dynamicBlock).Status.Equals(TrafficLight.LightStatus.YellowRed) && Character.Equals(character.aggresive))
                        {
                            AllowedVelocity = MaxVelocity;
                            reactedOnDynObjact = true;
                        }
                    }
                }
                if (!reactedOnDynObjact && ActVelocity == 0 && AllowedVelocity == 0)
                    AllowedVelocity = MaxVelocity;

                #endregion

                calcnewPosition(vehicle);
                calcNewVelocity();
            }
            else
            {
                if (distanceTodriveBeside > 0)
                {
                    if (switchLaneStartRotation == 0)
                    {
                        // Init rotation
                        if (switchLaneCount == 0 && distanceTodriveBeside > 32)
                            vehicle.Rotation = 360;
                        else if (switchLaneCount == 0)
                            vehicle.Rotation = 0;

                        if (switchLaneCount < 8 && distanceTodriveBeside > 32)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation -= 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X -= 4;
                            vehicle.Y += 4;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside > 32)
                        {
                            distanceTodriveBeside -= 4;
                            vehicle.Rotation = 0;
                            vehicle.X -= ActVelocity;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside <= 32)
                        {
                            switchLaneCount = 0;
                        }
                        else if (switchLaneCount < 8)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation += 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X -= 4;
                            vehicle.Y -= 4;
                        }
                    }
                    else if (switchLaneStartRotation == 90)
                    {
                        // Init rotation
                        if (switchLaneCount < 8 && distanceTodriveBeside > 32)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation -= 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X -= 4;
                            vehicle.Y -= 4;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside > 32)
                        {
                            distanceTodriveBeside -= 4;
                            vehicle.Rotation = 90;
                            vehicle.Y -= ActVelocity;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside <= 32)
                        {
                            switchLaneCount = 0;
                        }
                        else if (switchLaneCount < 8)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation += 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X += 4;
                            vehicle.Y -= 4;
                        }
                    }
                    else if (switchLaneStartRotation == 180)
                    {
                        if (switchLaneCount < 8 && distanceTodriveBeside > 32)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation -= 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X += 4;
                            vehicle.Y -= 4;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside > 32)
                        {
                            distanceTodriveBeside -= 4;
                            vehicle.Rotation = 180;
                            vehicle.X += ActVelocity;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside <= 32)
                        {
                            switchLaneCount = 0;
                        }
                        else if (switchLaneCount < 8)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation += 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X += 4;
                            vehicle.Y += 4;
                        }
                    }
                    else if (switchLaneStartRotation == 270)
                    {
                        if (switchLaneCount < 8 && distanceTodriveBeside > 32)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation -= 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X += 4;
                            vehicle.Y += 4;
                        }
                        else if (switchLaneCount >= 8 && distanceTodriveBeside > 32)
                        {
                            distanceTodriveBeside -= 4;
                            vehicle.Rotation = 270;
                            vehicle.Y += ActVelocity;
                        }
                        else if (switchLaneCount > 8 && distanceTodriveBeside <= 32)
                        {
                            switchLaneCount = 0;
                        }
                        else if (switchLaneCount < 8)
                        {
                            if (switchLaneCount < 3)
                                vehicle.Rotation += 15;
                            distanceTodriveBeside -= 4;
                            vehicle.X -= 4;
                            vehicle.Y += 4;
                        }
                    }

                    switchLaneCount++;
                }
                else
                {
                    vehicle.Rotation = switchLaneStartRotation;
                    reactonBrokenVehicleModeOn = false;
                }
            }
        }

        /// <summary>
        /// Method to calculate the distance between the actual vehicle and a given dynamic block
        /// </summary>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <param name="block">the dynamic object for which d´the distance should be calculated</param>
        /// <returns>double</returns>
        private double calcDistanceForSameDir(Vehicle vehicle, DynamicBlock block)
        {
            if (block.GetType().Equals(typeof(TrafficLight)))
                return Math.Sqrt(Math.Pow(vehicle.X - block.X, 2) + Math.Pow(vehicle.Y - block.Y, 2));

            else if (block.GetType().Equals(typeof(Vehicle)))
            {
                double rotationdifference = vehicle.Rotation - block.Rotation;

                if (rotationdifference < 0)
                    rotationdifference += 360;

                // tryout way
                // 0 degree or 180 degree
                if (rotationdifference >= 0 && rotationdifference < 45 || rotationdifference >= 315 && rotationdifference < 360 || rotationdifference >= 135 && rotationdifference < 225)
                    return Math.Sqrt(Math.Pow((vehicle.X + vehicle.Length / 2) - (block.X + ((Vehicle)block).Length / 2), 2) + Math.Pow((vehicle.Y + vehicle.Length / 2) - (block.Y + ((Vehicle)block).Length / 2), 2));
                // around 90 degree or 270 degree
                else
                    return Math.Sqrt(Math.Pow((vehicle.X + vehicle.Length / 2) - block.X, 2) + Math.Pow(vehicle.Y - block.Y, 2)) - vehicle.Length / 2 - ((Vehicle)block).Width / 2;
            }
            return 0;
        }

        /// <summary>
        /// Method to calculate if a given vehicle can react within the given distance 
        /// </summary>
        /// <param name="distance">distance between an object and the given vehicle</param>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <returns>bool</returns>
        private bool isBlockInReactionSpan(double distance, Vehicle vehicle)
        {
            if (distance < vehicle.Length + ActVelocity && distance > vehicle.Length / 2 && distance - ((ActVelocity * ActVelocity) / (2 * MaxDeceleration)) >= 0 || ActVelocity == 0)
                //if ((distance - ((ActVelocity * ActVelocity) / (2 * MaxDeceleration)) >= 0 || ActVelocity == 0) && distance < 50)
                return true;

            return false;
        }

        /// <summary>
        /// Method to calculate if a given vehicle can react on another vehicle with the given distance 
        /// </summary>
        /// <param name="distance">distance between an object and the given vehicle</param>
        /// <param name="ownVehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <param name="otherVehicle">the other vehicle</param>
        /// <returns>bool</returns>
        private bool isBlockInReactionSpan(double distance, Vehicle ownVehicle, Vehicle otherVehicle)
        {
            if (distance < (ownVehicle.Length + otherVehicle.Length + ownVehicle.driver.ActVelocity / 2) && distance - ((ActVelocity * ActVelocity) / (2 * MaxDeceleration)) >= 0 || ActVelocity == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Method to check if a dynamic block is in front of the vehicle
        /// </summary>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <param name="dynamicBlock">dynamic block which should be checked</param>
        /// <returns>bool</returns>
        private bool isDynamicBlockAhead(Vehicle vehicle, DynamicBlock dynamicBlock)
        {
            bool isAhead = false;
            // mathematical way:

            // tryout way
            // 0 degree
            if ((vehicle.Rotation >= 0 && vehicle.Rotation < 45 || vehicle.Rotation >= 315 && vehicle.Rotation <= 360) && vehicle.X > dynamicBlock.X)
                isAhead = true;
            // around 90 degree
            else if ((vehicle.Rotation >= 45 && vehicle.Rotation < 135) && vehicle.Y > dynamicBlock.Y)
                isAhead = true;
            // around 180 degree
            else if ((vehicle.Rotation >= 135 && vehicle.Rotation < 225) && vehicle.X < dynamicBlock.X)
                isAhead = true;
            // 270 degree
            else if (vehicle.Y > dynamicBlock.Y)
                isAhead = true;

            return isAhead;
        }

        /// <summary>
        /// Method to calculate the new position of the vehicle within the map. Belongs to actual velocity
        /// </summary>
        /// <param name="vehicle">actual vehicle to which the vehicle agent belongs</param>
        /// <returns>void</returns>
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

        /// <summary>
        /// Method to calculate the new velocity of the vehicle within the map. Belongs to actual acceleration or deceleration
        /// </summary>
        /// <returns>void</returns>
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
            if (newVelocity < 0)
                newVelocity = 0;
            ActVelocity = newVelocity;
        }

        /// <summary>
        /// Method to calculate the new acceleration of the vehicle within the map. The new acceleration depends on if the vehicle should be accelerate or decelerate
        /// </summary>
        /// <param name="accelerate">flag to signal if vehicle should accelerate</param>
        /// <param name="decelerate">flag to signal if vehicle should decelerate</param>
        /// <param name="fieldsToBreak">distance in blocks in which the vehicle should stand. Not used at the moment</param>
        /// <returns>void</returns>
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
                else if (ActVelocity > 0 && ActAcceleration < MaxDeceleration)
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

        /// <summary>
        /// Method to initialize the distances around the vehicle in which the driver should see street blocks
        /// </summary>
        /// <returns>void</returns>
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

        /*public virtual SortedDictionary<double, List<DynamicBlock>> loadDynamicBlockList(Vehicle vehicle)
        {
            initFieldDirection();
            Simulation.Simulator simu = Simulation.Simulator.Instance;

            SortedDictionary<double, List<DynamicBlock>> dynamicListWithDistance = simu.allDynamicObjectsInRange(vehicle.X, vehicle.Y, vehicle.Rotation, mapFieldsInDirection);
            return dynamicListWithDistance;
        }*/

        /// <summary>
        /// Method to load the list of dynamic objects
        /// </summary>
        /// <returns>List<DynamicBlock></returns>
        public virtual List<DynamicBlock> loadDynamicBlockList(Vehicle vehicle)
        {
            initFieldDirection();

            return Simulation.Simulator.Instance.allDynamicObjects;
        }

        /// <summary>
        /// Method to load the street map
        /// </summary>
        /// <returns>List<List<StreetBlock>></returns>
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
