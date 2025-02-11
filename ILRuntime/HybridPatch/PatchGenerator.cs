using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ILRuntime.Hybrid
{ 
    public class PatchGenerator
    {
        AssemblyHashInfo asmInfo;
        public PatchGenerator(Stream srcAsm, Stream patchedAsm)
        {
            LoadAssemblyHashInfo(srcAsm);
        }

        public PatchGenerator(AssemblyHashInfo info, Stream patchAsm)
        {
            this.asmInfo = info;
        }

        void LoadAssemblyHashInfo(Stream srcAsm)
        {
            var def = AssemblyDefinition.ReadAssembly(srcAsm);
            asmInfo = AssemblyHashInfo.BuildHashInfo(def);
        }

        public void Analyze()
        {

        }

        public void SavePatch(Stream stream)
        {

        }
    }
}
