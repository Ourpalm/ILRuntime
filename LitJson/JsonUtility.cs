using System;
using System.Collections.Generic;
using System.Reflection;

namespace LitJson
{
    internal class JsonUtility
    {
        public static IEnumerable<PropertyInfo> GetPropertyInfos(Type type)
        {
            var infos = new List<PropertyInfo>();

            var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < propertyInfos.Length; i++)
            {
                var info = propertyInfos[i];

                if (info.IsDefined(typeof(JsonIgnoreAttribute), true) == false)
                {
                    infos.Add(info);
                }
            }

            return infos;
        }

        public static IEnumerable<FieldInfo> GetFieldInfos(Type type)
        {
            var infos = new List<FieldInfo>();

            var fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < fieldInfos.Length; i++)
            {
                var info = fieldInfos[i];

                if (info.IsDefined(typeof(JsonIgnoreAttribute), true) == false)
                {
                    infos.Add(info);
                }
            }

            return infos;
        }
    }
}