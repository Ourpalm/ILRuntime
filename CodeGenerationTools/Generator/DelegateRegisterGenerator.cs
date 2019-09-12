using System;
using CodeGenerationTools.Generator.Base;
using ILRuntime.Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class DelegateRegisterGenerator : GeneratorBase<object>
    {
        public override bool LoadData(object data)
        {
            if (data == null)
                return false;

            if (data is Type)
            {
                var type = (Type)data;
                var method = type.GetMethod("Invoke");
                if (method == null)
                    return false;

                //var tmpd = method.ReturnType == typeof(void) ? _actionRegisterTmpd : _functionRegisterTmpd;
                var argsType = "";
                var returnType = method.ReturnType == typeof(void) ? "" : method.ReturnType.Name;
                foreach (var param in method.GetParameters())
                {
                    argsType += param.ParameterType.Name + ",";
                }

                if (method.ReturnType != typeof(void))
                    argsType += returnType;
                argsType = argsType.Trim(',');
                SetKeyValue("{$argsType}", argsType);
            }
            else if (data is TypeReference)
            {
                var tr = (TypeReference)data;
                var argsType = "";
                var gtype = (GenericInstanceType)tr;
                foreach (var param in gtype.GenericArguments)
                {
                    if (param != null)
                        argsType += param.FullName + ",";
                }
                argsType = argsType.Trim(',');
                SetKeyValue("{$argsType}", argsType);
            }
            else
                return false;

            return true;
        }
    }
}
