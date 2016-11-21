using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;
using Mono.Cecil;

namespace CodeGenerationTools.Generator
{
    public class AdaptorRegisterGenerator : GeneratorBase<TypeDefinition>
    {
        public override bool LoadData(TypeDefinition data)
        {
            if (data == null)
                return false;
            SetKeyValue("{$TypeName}", data.Name);
            return true;
        }
    }
}
