using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools
{
    public class DelegateConveterGenerator : GeneratorBase<Type>
    {
        public override bool LoadData(Type type)
        {
            if (type == null)
                return false;

            var method = type.GetMethod("Invoke");
            var argsType = "";
            var args = "";
            var returnType = method.ReturnType == typeof(void) ? "" : method.ReturnType.Name;
            foreach (var param in method.GetParameters())
            {
                argsType += param.ParameterType.Name + ",";
                args += param.Name + ",";
            }
            argsType = argsType.Trim(',');
            args = args.Trim(',');
            SetKeyValue("{$DelegateName}", type.FullName);
            SetKeyValue("{$argsType}", argsType);
            SetKeyValue("{$args}", args);
            if (method.ReturnType != typeof(void))
                SetKeyValue("{$returnType}", returnType);

            return true;
        }
    }
}
