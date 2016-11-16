using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools.Generator
{
    public class AdaptorRegisterGenerator : GeneratorBase<Type>
    {
        public override bool LoadData(Type data)
        {
            if (data == null)
                return false;
            SetKeyValue("{$TypeName}", data.Name);
            return true;
        }
    }
}
