using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class VirtualMethodGenerator : GeneratorBase<MethodDefinition>
    {
        public override bool LoadData(MethodDefinition methodInfo)
        {
            if (methodInfo == null)
                return false;

            var argStr = "";
            var argNoTypeStr = "";
            SetKeyValue("{$VMethodName}", methodInfo.Name);
            foreach (var pInfo in methodInfo.Parameters)
            {
                argStr += pInfo.ParameterType.Name + " " + pInfo.Name + ",";
                argNoTypeStr += pInfo.Name + ",";
            }
            argStr = argStr.Trim(',');
            argNoTypeStr = argNoTypeStr.Trim(',');
            SetKeyValue("{$args}", argStr);
            SetKeyValue("{$args_count}", methodInfo.Parameters.Count.ToString());
            SetKeyValue("{$args_no_type}", argNoTypeStr);
            SetKeyValue("{$comma}", argStr == "" ? "" : ",");
            SetKeyValue("{$modifier}", GetAccessmodifier(methodInfo));//Accessmodifier().ToString().ToLower());

            if (methodInfo.ReturnType.FullName != "System.Void")
                SetKeyValue("{$returnType}", methodInfo.ReturnType.Name);

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


        //Private				= 0x0001,	// Accessible only by the parent type
        //FamANDAssem			= 0x0002,	// Accessible by sub-types only in this Assembly
        //Assembly			    = 0x0003,	// Accessibly by anyone in the Assembly
        //Family				= 0x0004,	// Accessible only by type and sub-types
        //FamORAssem			= 0x0005,	// Accessibly by sub-types anywhere, plus anyone in assembly
        //Public				= 0x0006,	// Accessibly by anyone who has visibility to this scope
    }
}
