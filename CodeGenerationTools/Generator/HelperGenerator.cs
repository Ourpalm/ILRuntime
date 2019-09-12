using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using ILRuntime.Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class HelperGenerator : GeneratorBase<Tuple<Dictionary<string, TypeDefinition>, Dictionary<string, TypeDefinition>, Dictionary<string, TypeReference>>>
    {
        private string _filePath;
        private DelegateConveterGenerator _dcg;
        private DelegateRegisterGenerator _drg;
        private AdaptorRegisterGenerator _arg;

        public override bool LoadTemplateFromFile(string filePath)
        {
            _filePath = Path.GetDirectoryName(filePath);
            _dcg = new DelegateConveterGenerator();
            _drg = new DelegateRegisterGenerator();
            _arg = new AdaptorRegisterGenerator();

            return base.LoadTemplateFromFile(filePath);
        }

        public override bool LoadData(Tuple<Dictionary<string, TypeDefinition>, Dictionary<string, TypeDefinition>, Dictionary<string, TypeReference>> data)
        {
            if (data?.Item1 == null)
                return false;
            if (data.Item2 == null)
                return false;
            if (data.Item3 == null)
                return false;

            string nsStr = null;

            var adptorStr = "";
            foreach (var type in data.Item1.Values)
            {
                if (nsStr == null)
                    nsStr = type.Namespace;
                adptorStr += CreateAdaptorInit(type);
            }
            SetKeyValue("{$AdaptorInit}", adptorStr);

            var delegateStr = "";
            foreach (var type in data.Item2.Values)
            {
                if (nsStr == null)
                    nsStr = type.Namespace;
                delegateStr += CreateDelegateConvertorInit(type);
            }
            SetKeyValue("{$DelegateInit}", delegateStr);

            var delegateRegStr = "";
            foreach (var val in data.Item3.Values)
            {
                delegateRegStr += CreateDelegateRegisterInit(val);
            }
            SetKeyValue("{$DelegateRegInit}", delegateRegStr);

            SetKeyValue("{$Namespace}", nsStr);

            return true;
        }

        private string CreateAdaptorInit(TypeDefinition type)
        {
            _arg.InitFromFile(_filePath + "/adaptor_register.tmpd", type);
            return _arg.Generate();
        }

        private string CreateDelegateRegisterInit(object type)
        {
            string tmpd = null;
            if (type is Type)
            {
                var t = (Type)type;
                var method = t.GetMethod("Invoke");
                if (method == null)
                    return "";
                tmpd = method.ReturnType == typeof(void) ? "action_register.tmpd" : "function_register.tmpd";
            }
            else if (type is TypeReference)
            {
                var tr = (TypeReference)type;
                tmpd = tr.FullName.Contains("Action") ? "action_register.tmpd" : "function_register.tmpd";
            }
            _drg.InitFromFile(_filePath + Path.AltDirectorySeparatorChar + tmpd, type);

            return _drg.Generate();
        }

        private string CreateDelegateConvertorInit(TypeDefinition type)
        {
            var method = type.Methods.FirstOrDefault(m => m.Name == "Invoke");//GetMethod("Invoke");
            if (method == null)
                return "";
            var tmpd = method.ReturnType.FullName == "System.Void" ? "delegate_void.tmpd" : "delegate_return.tmpd";// == typeof(void) ? "delegate_void.tmpd" : "delegate_return.tmpd";
            _dcg.InitFromFile(_filePath + Path.AltDirectorySeparatorChar + tmpd, type);
            return _dcg.Generate();
        }
    }
}
