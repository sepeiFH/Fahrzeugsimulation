using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Simulator.Simulation.Base;

namespace Simulator.Simulation
{
    public static class BlockMapping
    {
        public static List<Block> Blocks { get; set; }

        /*private static List<int> roadSignList = null;
        public static List<int> RoadSignList {
            get
            {
                if (roadSignList == null)
                {
                    roadSignList = new List<int>();
                    foreach (RoadSigns roadSigns in Enum.GetValues(typeof(RoadSigns)))
                        roadSignList.Add((int)roadSigns);
                }
                    
                return roadSignList;
            }
        }*/

        static BlockMapping()
        {
                Blocks = ReflectiveEnumerator.GetEnumerableOfType<Block>().ToList();
        }

    }


    #region Blocks
    public class BlockGrass : Block
    {
        public BlockGrass()
        {
            GID = 2;
            Type = BlockType.StaticBlock;
        }
    }


    public class BlockStreetLeftToRight : StreetBlock
    {
        public BlockStreetLeftToRight()
        {
            GID = 3;
            Type = BlockType.StaticBlock;
            Direction = StreetDirection.LeftToRight;
        }
    }


    public class BlockStreetRightToLeft : StreetBlock
    {
        public BlockStreetRightToLeft()
        {
            GID = 4;
            Type = BlockType.StaticBlock;
            Direction = StreetDirection.RightToLeft;
        }
    }


    public class BlockStreetTopToDown : StreetBlock
    {
        public BlockStreetTopToDown()
        {
            GID = 5;
            Type = BlockType.StaticBlock;
            Direction = StreetDirection.TopToDown;
        }
    }


    public class BlockStreetDownToTop : StreetBlock
    {
        public BlockStreetDownToTop()
        {
            GID = 6;
            Type = BlockType.StaticBlock;
            Direction = StreetDirection.DownToTop;
        }
    }


    public class CrossingElement1 : CrossingElement
    {
        public CrossingElement1()
        {
            GID = 7;
            Type = BlockType.StaticBlock;
        }
    }

    /*public enum RoadSigns
    {
        TrafficLightRed = 12,
        TrafficLightYellow = 13,
        TrafficLightGreen = 14,
    }*/
    #endregion


    #region Helper Classes
    public static class ReflectiveEnumerator
    {
        static ReflectiveEnumerator() { }

        public static IEnumerable<T> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class, IComparable<T>
        {
            List<T> objects = new List<T>();
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                objects.Add((T)Activator.CreateInstance(type, constructorArgs));
            }
            objects.Sort();
            return objects;
        }
    }

    #endregion

}
