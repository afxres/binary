using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafePrimitiveConverter<T> : UnsafeConverter<T, T> where T : unmanaged
    {
        public override void OfValue(ref byte location, T item) => Endian<T>.Set(ref location, item);

        public override T ToValue(ref byte location) => Endian<T>.Get(ref location);
    }
}
