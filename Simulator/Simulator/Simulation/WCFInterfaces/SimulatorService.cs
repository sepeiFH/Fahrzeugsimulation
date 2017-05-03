using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Simulator.Simulation.Base;

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
            return SimulatorServiceHelpder.GetDynamicObjects(simu);
        }
        
        public string GetTrafficInitData()
        {
            return trafficString;
        }

        public List<TrafficLightGroupContract> GetTrafficLightGroups()
        {
            return SimulatorServiceHelpder.GetTrafficLightGroups(simu);
        }
        public void SetTrafficLightUpdate(List<TrafficLightContract> trafficlightList)
        {
            SimulatorServiceHelpder.SetTrafficLightUpdate(simu, trafficlightList);
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

    public class SimulatorServiceHelpder
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
                    TrafficLightContract tempLight = new TrafficLightContract() { Status = convertStatus(roadSign.Status), Direction = convertDirection(roadSign.Rotation),ID=roadSign.ID, PosX=roadSign.X, PosY=roadSign.Y};
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

        public static void SetTrafficLightUpdate(Simulator simu, List<TrafficLightContract> trafficlightList)
        {
            foreach(TrafficLightContract trafficLight in trafficlightList)
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

        private static TrafficLightDirection convertDirection(double rotation)
        { 
            return rotation == 0 ? TrafficLightDirection.Top : (rotation == 90 ? TrafficLightDirection.Right : (rotation == 180 ? TrafficLightDirection.Bottom : TrafficLightDirection.Left));
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
    public enum TrafficLightDirection
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
