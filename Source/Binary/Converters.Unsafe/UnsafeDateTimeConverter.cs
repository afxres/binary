using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeDateTimeConverter : UnsafeConverter<DateTime, Block08>
    {
        public override void OfValue(ref byte location, DateTime item) => Endian<long>.Set(ref location, item.ToBinary());

        public override DateTime ToValue(ref byte location) => DateTime.FromBinary(Endian<long>.Get(ref location));
    }
}
