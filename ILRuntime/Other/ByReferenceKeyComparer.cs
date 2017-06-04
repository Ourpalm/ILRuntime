using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class ByReferenceKeyComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y)
    {
        return object.ReferenceEquals(x, y);
    }

    public int GetHashCode(T obj)
    {
        return RuntimeHelpers.GetHashCode(obj);
    }
}
