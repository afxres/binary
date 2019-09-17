using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeTimeSpanConverter : UnsafeConverter<TimeSpan, Block08>
    {
        public override void OfValue(ref byte location, TimeSpan item) => Endian<long>.Set(ref location, item.Ticks);

        public override TimeSpan ToValue(ref byte location) => new TimeSpan(ticks: Endian<long>.Get(ref location));
    }
}
