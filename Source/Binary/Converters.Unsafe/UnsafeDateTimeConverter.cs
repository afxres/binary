using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeDateTimeConverter : UnsafeConverter<DateTime, Block08>
    {
        protected override void Of(ref byte location, DateTime item) => Endian<long>.Set(ref location, item.ToBinary());

        protected override DateTime To(ref byte location) => DateTime.FromBinary(Endian<long>.Get(ref location));
    }
}
