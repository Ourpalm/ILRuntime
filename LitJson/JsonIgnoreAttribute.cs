using System;

namespace LitJson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class JsonIgnoreAttribute : Attribute
    {

    }
}