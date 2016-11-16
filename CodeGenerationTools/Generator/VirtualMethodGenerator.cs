using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools.Generator
{
    public class VirtualMethodGenerator : GeneratorBase<MethodInfo>
    {
        public override bool LoadData(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return false;
            
            var argStr = "";
            var argNoTypeStr = "";
            SetKeyValue("{$VMethodName}", methodInfo.Name);
            foreach (var pInfo in methodInfo.GetParameters())
            {
                argStr += pInfo.ParameterType.Name + " " + pInfo.Name + ",";
                argNoTypeStr += pInfo.Name + ",";
            }
            argStr = argStr.Trim(',');
            argNoTypeStr = argNoTypeStr.Trim(',');
            SetKeyValue("{$args}", argStr);
            SetKeyValue("{$args_no_type}", argNoTypeStr);
            SetKeyValue("{$comma}", argStr == "" ? "" : ",");
            SetKeyValue("{$modifier}", methodInfo.Accessmodifier().ToString().ToLower());

            if (methodInfo.ReturnType != typeof(void))
                SetKeyValue("{$returnType}", methodInfo.ReturnType.Name);

            return true;
        }
    }
}
