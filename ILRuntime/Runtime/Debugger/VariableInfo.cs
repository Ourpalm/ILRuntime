using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILRuntime.Runtime.Debugger
{
    public enum VariableTypes
    {
        Normal,
        FieldReference,
        PropertyReference,
        TypeReference,
        IndexAccess,
        Invocation,        
        Error,
        NotFound,
    }

    public class VariableReference
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public int Offset { get; set; }
        public string Name { get; set; }
        public VariableReference Parent { get; set; }
    }

    public class VariableInfo
    {
        public long Address { get; set; }
        public VariableTypes Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
        public string Value { get; set; }
        public bool Expandable { get; set; }
        public int Offset { get; set;}

        public static VariableInfo NullReferenceExeption = new VariableInfo
        {
            Type = VariableTypes.Error,
            Name = "",
            TypeName = "",
            Value = "NullReferenceException"
        };

        public static VariableInfo GetCannotFind(string name)
        {
            var res = new VariableInfo
            {
                Type = VariableTypes.NotFound,
                TypeName = "",
            };
            res.Name = name;
            res.Value = string.Format("Cannot find {0} in current scope.", name);

            return res;
        }
    }
}
