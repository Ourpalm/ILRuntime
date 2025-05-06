using ILRuntime.Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Text;

namespace ILRuntime.Hybrid
{
    internal struct TypeInjectionContext
    {
        public bool IsGenericType;
        public FieldDefinition ExtraFieldILType;
        public FieldDefinition ExtraFieldILTypeGenericDefinition;
        public FieldReference ExtraFieldILTypeGenericInstance;
        public FieldReference ExtraFieldsObject;
        public TypeDefinition GenericTypeDefinition;
        public TypeDefinition DeclaringType;
        public TypeDefinition DeclaringTypeGenericDefinition;
        public TypeReference DeclaringTypeGenericInstance;
        public MethodReference InitializeGenericInstance;
        public FieldReference InitializedPatchHash;
        public MethodReference InstantiateMethod;
        public bool IsRoot;
        public List<MethodRedirectionInjectionContext> MethodRedirections;
    }

    internal struct MethodRedirectionInjectionContext
    {
        public FieldDefinition ILMethod;
        public FieldReference ILMethodGenericInstance;
        public FieldDefinition ILMethodGenericDefinition;
    }
}
