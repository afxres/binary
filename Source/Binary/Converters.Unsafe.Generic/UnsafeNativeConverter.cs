using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;

namespace Mikodev.Binary.Converters.Unsafe.Generic
{
    internal sealed class UnsafeNativeConverter<T> : UnsafeAbstractConverter<T, T> where T : unmanaged
    {
        protected override void Of(ref byte location, T item) => Endian<T>.Set(ref location, item);

        protected override T To(ref byte location) => Endian<T>.Get(ref location);
    }
}
