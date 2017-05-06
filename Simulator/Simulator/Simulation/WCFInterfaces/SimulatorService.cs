using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Simulator.Simulation.Base;
using Simulator.Simulation.Utilities;

namespace Simulator.Simulation.WCFInterfaces
{
    #region ServiceContracts

    #region MapService
    [ServiceContract]
    public interface SimulatorServiceMap
    {
        [OperationContract]
        string GetMap();


        [OperationContract]
        List<BlockObjectContract> GetDynamicObjects();

        [OperationContract]
        void ToggleBrokenItem(BlockObjectContract item);

        //[OperationContract]
        //List<RoadSign> getRoadSigns();
    }

    public class SimulatorService : SimulatorServiceMap, SimulatorServiceTrafficControl
    {
        public static string mapString;
        public static string trafficString;
        public static Simulator simu;
        public string GetMap()
        {
            return mapString;
        }

        public List<BlockObjectContract> GetDynamicObjects()
        {
            return SimulatorServiceHelper.GetDynamicObjects(simu);
        }

        public void ToggleBrokenItem(BlockObjectContract item)
        {
            SimulatorServiceHelper.ToggleBroken(simu, item);
        }

        public string GetTrafficInitData()
        {
            return trafficString;
        }

        public List<TrafficLightGroupContract> GetTrafficLightGroups()
        {
            return SimulatorServiceHelper.GetTrafficLightGroups(simu);
        }
        public void SetTrafficLightUpdate(List<TrafficLightContract> trafficlightList)
        {
            SimulatorServiceHelper.SetTrafficLightUpdate(simu, trafficlightList);
        }
        /*public List<RoadSign> getRoadSigns()
{
   return allRoadSigns;
}*/
    }

    #endregion

    #region TrafficlightService
    [ServiceContract]
    public interface SimulatorServiceTrafficControl
    {
        [OperationContract]
        string GetTrafficInitData();

        [OperationContract]
        List<TrafficLightGroupContract> GetTrafficLightGroups();

        [OperationContract]
        void SetTrafficLightUpdate(List<TrafficLightContract> trafficlightList);
    }
    #endregion

    #endregion

    #region HelperClasses

    public class SimulatorServiceHelper
    {
        public static List<BlockObjectContract> GetDynamicObjects(Simulator simu)
        {
            List<BlockObjectContract> tempContracts = new List<BlockObjectContract>();
            try
            {
                foreach (DynamicBlock obj in simu.allDynamicObjects)
                {
                    //if (obj is Vehicle)
                    tempContracts.Add(new BlockObjectContract() { GID = obj.GID, Rotation = obj.Rotation, X = obj.X, Y = obj.Y });
                }
            } catch (Exception e) { }

            return tempContracts;
        }

        public static List<TrafficLightGroupContract> GetTrafficLightGroups(Simulator simu)
        {
            List<TrafficLightGroupContract> tempGroupList = new List<TrafficLightGroupContract>();

            try
            {
                foreach (TrafficLight roadSign in simu.roadSignList)
                {
                    TrafficLightGroupContract tempGroup = tempGroupList.Find(X => X.ID.Equals(roadSign.RoadSignGroup));
                    TrafficLightContract tempLight;
                    if (Simulator.EmergencyModeActive)
                        tempLight = new TrafficLightContract() { Status = TrafficLightStatus.Red, Position = convertPosition(roadSign.Rotation), ID = roadSign.ID, PosX = roadSign.X, PosY = roadSign.Y };
                    else
                        tempLight = new TrafficLightContract() { Status = convertStatus(roadSign.Status), Position = convertPosition(roadSign.Rotation),ID=roadSign.ID, PosX=roadSign.X, PosY=roadSign.Y};
                    if (tempGroup != null)
                    {
                        tempGroup.TrafficLights.Add(tempLight);
                    }
                    else
                    {
                        tempGroup = new TrafficLightGroupContract() { ID = roadSign.RoadSignGroup, TrafficLights = new List<TrafficLightContract>() };
                        tempGroup.TrafficLights.Add(tempLight);
                        tempGroupList.Add(tempGroup);
                    }
                }
            }
            catch (Exception e) { }

            return tempGroupList;
        }

        public static void ToggleBroken(Simulator simu, BlockObjectContract item)
        {
            double smallestDistance = Double.MaxValue;
            DynamicBlock tempItem = null;
            foreach (DynamicBlock currentItem in simu.allDynamicObjects.Where(x => x.GID == item.GID))
            {
                double currentDistance = Utils.GetDistance(currentItem.X, currentItem.Y, item.X, item.Y);
                if (currentDistance < smallestDistance)
                {
                    tempItem = currentItem;
                    smallestDistance = currentDistance;
                }
            }
            if (tempItem != null)
                tempItem.IsBroken = !tempItem.IsBroken;
        }

        public static void SetTrafficLightUpdate(Simulator simu, List<TrafficLightContract> trafficlightList)
        {
            Simulator.EmergencyModeActive = false;
            simu.emergencyWatch.Restart();
            foreach (TrafficLightContract trafficLight in trafficlightList)
            {
                TrafficLight sign = (TrafficLight)simu.allDynamicObjects.Find(x => x.ID == trafficLight.ID);
                sign.Status = convertStatus(trafficLight.Status);
                sign.GID = (int)sign.Status;
            }
        }

        private static TrafficLight.LightStatus convertStatus(TrafficLightStatus status)
        {
            TrafficLight.LightStatus tempStatus;

            if (status == TrafficLightStatus.Green)
                tempStatus = TrafficLight.LightStatus.Green;
            else if (status == TrafficLightStatus.Red)
                tempStatus = TrafficLight.LightStatus.Red;
            else if (status == TrafficLightStatus.Yellow)
                tempStatus = TrafficLight.LightStatus.Yellow;
            else
                tempStatus = TrafficLight.LightStatus.YellowRed;

            return tempStatus;
        }
        private static TrafficLightStatus convertStatus(TrafficLight.LightStatus status)
        {
            TrafficLightStatus tempStatus;

            if (status == TrafficLight.LightStatus.Green)
                tempStatus = TrafficLightStatus.Green;
            else if (status == TrafficLight.LightStatus.Red)
                tempStatus = TrafficLightStatus.Red;
            else if (status == TrafficLight.LightStatus.Yellow)
                tempStatus = TrafficLightStatus.Yellow;
            else
                tempStatus = TrafficLightStatus.YellowRed;

            return tempStatus;
        }

        private static TrafficLightPosition convertPosition(double rotation)
        { 
            return rotation == 90 ? TrafficLightPosition.Top : (rotation == 180 ? TrafficLightPosition.Right : (rotation == 270 ? TrafficLightPosition.Bottom : TrafficLightPosition.Left));
        }
    }

    [DataContract]
    public class TrafficLightGroupContract
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public List<TrafficLightContract> TrafficLights { get; set; }
    }

    [DataContract]
    public class TrafficLightContract
    {
        [DataMember]
        public TrafficLightDirection Direction { get; set; }
        [DataMember]
        public TrafficLightPosition Position { get; set; }

        [DataMember]
        public TrafficLightStatus Status { get; set; }

        [DataMember]
        public TrafficLightContract Neighbor { get; set; }

        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public double PosX { get; set; }

        [DataMember]
        public double PosY { get; set; }
        //public Dictionary<string, string> TrafficLights { get; set; }
    }

    [DataContract]
    public enum TrafficLightStatus
    {
        [EnumMember]
        Red = TrafficLight.LightStatus.Red,
        [EnumMember]
        Yellow = TrafficLight.LightStatus.Yellow,
        [EnumMember]
        Green = TrafficLight.LightStatus.Green,
        [EnumMember]
        YellowRed = TrafficLight.LightStatus.YellowRed
    }

    [DataContract]
    public enum TrafficLightPosition
    {
        [EnumMember]
        Top,
        [EnumMember]
        Bottom,
        [EnumMember]
        Left,
        [EnumMember]
        Right
    }

    [DataContract]
    public enum TrafficLightDirection
    {
        [EnumMember]
        All,
        [EnumMember]
        Straight,
        [EnumMember]
        Right,
        [EnumMember]
        Left
    }

    [DataContract]
    public class BlockObjectContract
    {
        [DataMember]
        public double X { get; set; }

        [DataMember]
        public double Y { get; set; }

        [DataMember]
        public double Rotation { get; set; }

        [DataMember]
        public int GID { get; set; }
    }

    #endregion
}
