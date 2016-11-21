using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using Mono.Cecil;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace CodeGenerationTools.Generator
{
    public class AdaptorGenerator : GeneratorBase<TypeDefinition>
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

        public override bool LoadData(TypeDefinition type)
        {
            if (type == null)
                return false;
            if (type.IsInterface)
                return false;

            var methodsbody = "";
            var methods = type.Methods;
            foreach (var methodInfo in methods.Where(methodInfo => methodInfo.DeclaringType.FullName != "System.Object"))
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

            SetKeyValue("{$Namespace}", type.Namespace);
            SetKeyValue("{$ClassName}", type.Name);
            SetKeyValue("{$MethodArea}", methodsbody);

            var interfaceStr = "";
            foreach (var iface in type.Interfaces)
            {
                interfaceStr += CreateInterfaceAdaptor(iface.Resolve(), type);
            }

            SetKeyValue("{$Interface}", interfaceStr);

            return true;

            //if (type == null)
            //    return false;

            //if (type.IsInterface)
            //    return false;

            //var methodsbody = "";
            //var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            //foreach (var methodInfo in methods.Where(methodInfo => methodInfo.DeclaringType != typeof(object)))
            //{
            //    if (methodInfo.IsAbstract)
            //    {
            //        methodsbody += CreateAbstractMethod(methodInfo);
            //    }
            //    else if (methodInfo.IsVirtual && !methodInfo.IsFinal)
            //    {
            //        methodsbody += CreateVirtualMethod(methodInfo);
            //    }
            //}

            //SetKeyValue("{$ClassName}", type.Name);
            //SetKeyValue("{$MethodArea}", methodsbody);

            //var interfaceStr = "";
            //foreach (var iface in type.GetInterfaces())
            //{
            //    interfaceStr += CreateInterfaceAdaptor(iface, type);
            //}

            //SetKeyValue("{$Interface}", interfaceStr);

            //return true;

        }



        private string CreateInterfaceAdaptor(TypeDefinition iface, TypeDefinition type)
        {
            _ig.InitFromFile(_filePath + "/adaptor_interface.tmpd", new Tuple<TypeDefinition, TypeDefinition>(iface, type));
            return _ig.Generate();
        }

        private string CreateVirtualMethod(MethodDefinition methodInfo)
        {

            _vmg.InitFromFile(
                _filePath + (methodInfo.ReturnType.FullName == "System.Void"
                    ? "/method_virtual_void.tmpd"
                    : "/method_virtual_return.tmpd"),
                methodInfo);
            return _vmg.Generate();
        }

        private string CreateAbstractMethod(MethodDefinition methodInfo)
        {
            _amg.InitFromFile(
              _filePath + (methodInfo.ReturnType.FullName == "System.Void"
                ? "/method_abstract_void.tmpd"
                : "/method_abstract_return.tmpd"),
              methodInfo);
            return _amg.Generate();
        }
    }
}
