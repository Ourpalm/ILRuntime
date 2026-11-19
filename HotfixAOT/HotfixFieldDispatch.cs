using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using BindingFlags = System.Reflection.BindingFlags;
using Domain = ILRuntime.Runtime.Enviorment.AppDomain;
using FieldInfo = System.Reflection.FieldInfo;

namespace HotfixAOT
{
    [ILRuntimePatch]
    public class HotfixFieldDispatchTarget
    {
        public int[] Values = new int[3];
        public static int StaticValue = 3;
        private int injectedValue;
        private static int injectedStaticValue;

        public int UpdateInjectedFields(int value)
        {
            injectedValue = value;
#if PATCHED
            injectedStaticValue = value * 2;
#else
            injectedStaticValue = value;
#endif
            return injectedValue + injectedStaticValue;
        }
    }

    public class HotfixFieldDispatchTestCases
    {
        // Give an ordinary CLR field a token with the former injected-field bit set.
        static void SetCollidingFieldToken(CLRType type, FieldInfo field)
        {
            var fields = type.Fields;
            var mapping = (Dictionary<string, int>)typeof(CLRType)
                .GetField("fieldMapping", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(type);
            int token = 0x10000000;
            while (fields.ContainsKey(token))
                token++;
            mapping[field.Name] = token;
            fields[token] = field;
        }

        static unsafe bool RunInterpretedFieldAccess(bool isStatic)
        {
            var domain = new Domain();
            var targetType = typeof(HotfixFieldDispatchTarget);
            int originalStaticValue = HotfixFieldDispatchTarget.StaticValue;
            try
            {
                domain.AddReferenceAssembly(targetType.Assembly);
                var field = targetType.GetField(isStatic ? nameof(HotfixFieldDispatchTarget.StaticValue) : nameof(HotfixFieldDispatchTarget.Values));
                SetCollidingFieldToken((CLRType)domain.GetType(targetType), field);
                if (isStatic)
                    domain.RegisterTypeStaticFieldAccessor(targetType,
                        (index, interpreter, stack, objects) => throw new InvalidOperationException("CLR field routed to injected getter"),
                        (index, interpreter, stack, objects) => throw new InvalidOperationException("CLR field routed to injected setter"));

                var target = new HotfixFieldDispatchTarget();
                using (var module = ModuleDefinition.CreateModule("FieldDispatchProbe", ModuleKind.Dll))
                using (var stream = new MemoryStream())
                {
                    var probe = new TypeDefinition("", "FieldDispatchProbe", TypeAttributes.Public, module.TypeSystem.Object);
                    module.Types.Add(probe);
                    var method = new MethodDefinition("Access", MethodAttributes.Public | MethodAttributes.Static, module.TypeSystem.Int32);
                    probe.Methods.Add(method);
                    method.Parameters.Add(new ParameterDefinition(module.ImportReference(targetType)));
                    var il = method.Body.GetILProcessor();
                    var reference = module.ImportReference(field);
                    if (isStatic)
                    {
                        il.Emit(OpCodes.Ldc_I4_7);
                        il.Emit(OpCodes.Stsfld, reference);
                        il.Emit(OpCodes.Ldsfld, reference);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldfld, reference);
                        il.Emit(OpCodes.Ldlen);
                        il.Emit(OpCodes.Conv_I4);
                        il.Emit(OpCodes.Ldarg_0);
                        il.Emit(OpCodes.Ldc_I4_7);
                        il.Emit(OpCodes.Newarr, module.TypeSystem.Int32);
                        il.Emit(OpCodes.Stfld, reference);
                    }
                    il.Emit(OpCodes.Ret);
                    module.Write(stream);
                    stream.Position = 0;
                    domain.LoadAssembly(stream);
                    int result = (int)domain.Invoke("FieldDispatchProbe", "Access", null, target);
                    return isStatic ? result == 7 && HotfixFieldDispatchTarget.StaticValue == 7
                        : result == 3 && target.Values.Length == 7;
                }
            }
            finally
            {
                HotfixFieldDispatchTarget.StaticValue = originalStaticValue;
                domain.Dispose();
            }
        }

        public static IEnumerable<ITestCase> GetTestCases()
        {
            yield return new DelegateTestCase($"{nameof(HotfixFieldDispatchTestCases)}.InstanceFieldHash", _ => RunInterpretedFieldAccess(false));
            yield return new DelegateTestCase($"{nameof(HotfixFieldDispatchTestCases)}.StaticFieldHash", _ => RunInterpretedFieldAccess(true));
            yield return new DelegateTestCase($"{nameof(HotfixFieldDispatchTestCases)}.InjectedFields", patched =>
                new HotfixFieldDispatchTarget().UpdateInjectedFields(7) == (patched ? 21 : 14));
        }
    }
}
