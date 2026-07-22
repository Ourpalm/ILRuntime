using System;

namespace ILRuntime.Other
{
    /// <summary>
    /// Marks a CLR value type as Inline storage in Neo mode: the struct's fields live in the frame's
    /// primitive+reference segments (identical layout to IL value types), and CLR method invocations
    /// on the struct go through registered ValueTypeBinder redirections or generated CLR bindings.
    ///
    /// Semantics: opts the struct into Inline storage. Does NOT require the fields to be CLR-blittable
    /// in the strict sense — reference fields are allowed and go through the mStack reference segment.
    ///
    /// Applies only when compiled with ENABLE_NEO_MODE. Ignored in Legacy mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false)]
    public sealed class ILRuntimeBlittableAttribute : Attribute
    {
    }
}
