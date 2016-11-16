using System.Reflection;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools.Generator
{
    public class AbstractMethodGenerator : GeneratorBase<MethodInfo>
    {
        public override bool LoadData(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return false;

            string argStr = "";
            string argNoTypeStr = "";

            SetKeyValue("{$AMethodName}", methodInfo.Name);
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

            if (methodInfo.ReturnType == typeof(void)) return true;

            SetKeyValue("{$returnType}", methodInfo.ReturnType.Name);
            var returnStr = methodInfo.ReturnType.IsValueType ? "return 0;" : "return null;";
            SetKeyValue("{$returnDefault}", returnStr);

            return true;
        }
    }
}
