using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class InterfaceGenerator : GeneratorBase<Tuple<TypeDefinition, TypeDefinition>>
    {
        public override bool LoadData(Tuple<TypeDefinition, TypeDefinition> data)
        {
            if (data == null)
                return false;

            SetKeyValue("{$ClassName}", data.Item1.Name);
            SetKeyValue("{$AdaptorName}", data.Item2.Name + "Adaptor.Adaptor");

            return true;
        }
    }
}
