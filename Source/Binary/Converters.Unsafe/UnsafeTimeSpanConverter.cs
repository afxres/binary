using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeTimeSpanConverter : UnsafeAbstractConverter<TimeSpan, Block08>
    {
        protected override void Of(ref byte location, TimeSpan item) => Endian<long>.Set(ref location, item.Ticks);

        protected override TimeSpan To(ref byte location) => new TimeSpan(ticks: Endian<long>.Get(ref location));
    }
}
