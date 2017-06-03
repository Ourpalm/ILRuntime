using System;
using System.Collections.Generic;

/// <summary>
/// Fast comparer of Type objects, assumes there is at most one Type object for a given FullName
/// </summary>
public class DictionaryTypeKeyComparer : IEqualityComparer<Type>
{
    public bool Equals(Type x, Type y)
    {
        return x == y;
    }

    public int GetHashCode(Type obj)
    {
        return obj.FullName.GetHashCode();
    }
}
