using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Simulator.Simulation.Base;

namespace Simulator.Simulation.WCFInterfaces
{
    #region ServiceContracts
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

    public class SimulatorService : SimulatorServiceMap
    {
        public static string mapString;
        public static Simulator simu;
        public string GetMap()
        {
            return mapString;
        }

        public List<BlockObjectContract> GetDynamicObjects()
        {
            return SimulatorServiceHelpder.GetDynamicObjects(simu);
        }

        /*public List<RoadSign> getRoadSigns()
        {
            return allRoadSigns;
        }*/
    }
    #endregion

    #region HelperClasses

    public class SimulatorServiceHelpder
    {
        public static List<BlockObjectContract> GetDynamicObjects(Simulator simu)
        {
            List<BlockObjectContract> tempContracts = new List<BlockObjectContract>();
            foreach (DynamicBlock obj in simu.allDynamicObjects)
            {
                //if (obj is Vehicle)
                tempContracts.Add(new BlockObjectContract() { GID = obj.GID, Rotation = obj.Rotation, X = obj.X, Y = obj.Y });
            }

            return tempContracts;
        }
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
