using System.Reflection;
using CodeGenerationTools.Generator.Base;
using ILRuntime.Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class AbstractMethodGenerator : GeneratorBase<MethodDefinition>
    {
        public override bool LoadData(MethodDefinition methodInfo)
        {
            if (methodInfo == null)
                return false;

            string argStr = "";
            string argNoTypeStr = "";

            SetKeyValue("{$AMethodName}", methodInfo.Name);
            foreach (var pInfo in methodInfo.Parameters)//.GetParameters())
            {
                argStr += pInfo.ParameterType.Name + " " + pInfo.Name + ",";
                argNoTypeStr += pInfo.Name + ",";
            }
            argStr = argStr.Trim(',');
            argNoTypeStr = argNoTypeStr.Trim(',');
            SetKeyValue("{$args}", argStr);
            SetKeyValue("{$args_no_type}", argNoTypeStr);

            SetKeyValue("{$comma}", argStr == "" ? "" : ",");
            SetKeyValue("{$modifier}", GetAccessmodifier(methodInfo));//.Accessmodifier().ToString().ToLower());

            if (methodInfo.ReturnType.FullName == "System.Void") return true;

            SetKeyValue("{$returnType}", methodInfo.ReturnType.Name);
            var returnStr = methodInfo.ReturnType.IsValueType ? "return 0;" : "return null;";
            SetKeyValue("{$returnDefault}", returnStr);

            return true;
        }


        public string GetAccessmodifier(MethodDefinition method)
        {
            if (method.IsPrivate)
                return "private";
            if (method.IsFamilyOrAssembly)
                return "protected internal";//!--
            if (method.IsAssembly)
                return "internal";
            if (method.IsFamily)
                return "protected";
            if (method.IsPublic)
                return "public";
            //dont know what the hell 
            //if (method.IsFamilyAndAssembly)
            //    return "public";
            return "public";
        }
    }
}
