using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerationTools.Generator.Base;

namespace CodeGenerationTools.Generator
{
    public class InterfaceGenerator : GeneratorBase<Tuple<Type, Type>>
    {
        public override bool LoadData(Tuple<Type, Type> data)
        {
            if (data == null)
                return false;

            SetKeyValue("{$ClassName}", data.Item1.Name);
            SetKeyValue("{$AdaptorName}", data.Item2.Name + "Adaptor.Adaptor");

            return true;
        }
    }
}
