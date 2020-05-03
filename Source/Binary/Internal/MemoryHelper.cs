using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T EnsureHandleEndian<T>(T item, bool swap) where T : unmanaged
        {
            if (typeof(T) == typeof(short))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((short)(object)item);
            else if (typeof(T) == typeof(int))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((int)(object)item);
            else if (typeof(T) == typeof(long))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((long)(object)item);
            else
                throw new NotSupportedException();
        }
    }
}
