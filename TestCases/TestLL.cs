
//using CSEvilTestor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
namespace TestCases
{
    class XmlAttribute:Attribute
    {

    }

    class XmlFieldListAttribute: XmlAttribute
    {

    }

    class XmlFieldMapAttribute : XmlAttribute
    {
        
    }


    public class TestLL
    {
        [XmlFieldMapAttribute]
        public Dictionary<int, int> tempMap = new Dictionary<int, int>();

        public static string TestIsAssignableFrom()
        {
            Console.WriteLine("sign not XmlAttribute" + typeof(XmlFieldListAttribute).IsAssignableFrom(typeof(XmlAttribute)));
            Console.WriteLine("sign not Attribute" + typeof(XmlFieldListAttribute).IsAssignableFrom(typeof(Attribute)));

            Console.WriteLine("sign XmlAttribute " + typeof(XmlAttribute).IsAssignableFrom(typeof(XmlFieldListAttribute)));
            Console.WriteLine("sign Attribute " + typeof(Attribute).IsAssignableFrom(typeof(XmlFieldListAttribute)));
            //主要是这一条
            Console.WriteLine("sign Attribute " + typeof(Attribute).IsAssignableFrom(typeof(XmlAttribute)));

            Console.WriteLine("sub XmlAttribute " + typeof(XmlFieldListAttribute).IsSubclassOf(typeof(XmlAttribute)));
            Console.WriteLine("sub Attribute " + typeof(XmlFieldListAttribute).IsSubclassOf(typeof(Attribute)));
            return "Test finish!";
        }

        public static string TestILRuntimeTypeEquals()
        {
            FieldInfo fieldInfo = typeof(TestLL).GetField("tempMap");

            object[] xmlAttributeList = fieldInfo.GetCustomAttributes(typeof(XmlAttribute), true);
            //object[] xmlAttributeList = fieldInfo.GetCustomAttributes(typeof(XmlFieldListAttribute), true);
            Console.WriteLine(xmlAttributeList + ":" + xmlAttributeList.Count());
            foreach (var item in xmlAttributeList)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine(fieldInfo.IsDefined(typeof(XmlFieldMapAttribute), true));
            Console.WriteLine(fieldInfo.IsDefined(typeof(XmlFieldListAttribute), true));

            return "Test finish!";
        }
    }
}