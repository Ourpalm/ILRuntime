using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools.Generator
{
    public class AdaptorGenerator : GeneratorBase<Type>
    {
        private string _filePath;
        private VirtualMethodGenerator _vmg;
        private AbstractMethodGenerator _amg;
        private InterfaceGenerator _ig;

        public override bool LoadTemplateFromFile(string filePath)
        {
            _filePath = Path.GetDirectoryName(filePath);

            _vmg = new VirtualMethodGenerator();
            _amg = new AbstractMethodGenerator();
            _ig = new InterfaceGenerator();

            return base.LoadTemplateFromFile(filePath);
        }

        public override bool LoadData(Type type)
        {
            if (type == null)
                return false;

            if (type.IsInterface)
                return false;

            var methodsbody = "";
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var methodInfo in methods.Where(methodInfo => methodInfo.DeclaringType != typeof(object)))
            {
                if (methodInfo.IsAbstract)
                {
                    methodsbody += CreateAbstractMethod(methodInfo);
                }
                else if (methodInfo.IsVirtual && !methodInfo.IsFinal)
                {
                    methodsbody += CreateVirtualMethod(methodInfo);
                }
            }

            SetKeyValue("{$ClassName}", type.Name);
            SetKeyValue("{$MethodArea}", methodsbody);

            var interfaceStr = "";
            foreach (var iface in type.GetInterfaces())
            {
                interfaceStr += CreateInterfaceAdaptor(iface, type);
            }

            SetKeyValue("{$Interface}", interfaceStr);

            return true;
        }

        private string CreateInterfaceAdaptor(Type iface, Type type)
        {
            _ig.InitFromFile(_filePath + "/adaptor_interface.tmpd", new Tuple<Type, Type>(iface, type));
            return _ig.Generate();
        }

        private string CreateVirtualMethod(MethodInfo methodInfo)
        {
            _vmg.InitFromFile(
                _filePath + (methodInfo.ReturnType == typeof(void)
                    ? "/method_virtual_void.tmpd"
                    : "/method_virtual_return.tmpd"),
                methodInfo);
            return _vmg.Generate();
        }

        private string CreateAbstractMethod(MethodInfo methodInfo)
        {
            _amg.InitFromFile(
              _filePath + (methodInfo.ReturnType == typeof(void)
                ? "/method_abstract_void.tmpd"
                : "/method_abstract_return.tmpd"),
              methodInfo);
            return _amg.Generate();
        }
    }
}
