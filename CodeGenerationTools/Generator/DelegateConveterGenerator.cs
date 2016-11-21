using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using Mono.Cecil;

namespace CodeGenerationTools
{
    public class DelegateConveterGenerator : GeneratorBase<TypeDefinition>
    {
        public override bool LoadData(TypeDefinition type)
        {
            if (type == null)
                return false;

            var method = type.Methods.FirstOrDefault(m => m.Name == "Invoke");//type.GetMethod("Invoke");
            if (method == null)
                return false;

            var argsType = "";
            var args = "";
            var returnType = method.ReturnType.FullName == "System.Void" ? "" : method.ReturnType.Name;//== typeof(void) ? "" : method.ReturnType.Name;
            foreach (var param in method.Parameters)//.GetParameters())
            {
                argsType += param.ParameterType.Name + ",";
                args += param.Name + ",";
            }
            argsType = argsType.Trim(',');
            args = args.Trim(',');
            SetKeyValue("{$DelegateName}", type.FullName.Replace("/", "."));
            SetKeyValue("{$argsType}", argsType);
            SetKeyValue("{$args}", args);
            if (method.ReturnType.FullName != "System.Void")//!= typeof(void))
                SetKeyValue("{$returnType}", returnType);

            return true;
        }
    }
}
