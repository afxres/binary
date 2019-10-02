using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;
using int16 = System.Int16;
using int64 = System.Int64;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeDateTimeOffsetConverter : UnsafeAbstractConverter<DateTimeOffset, Block10>
    {
        protected override void Of(ref byte location, DateTimeOffset item)
        {
            Endian<int64>.Set(ref location, item.Ticks);
            Endian<int16>.Set(ref Memory.Add(ref location, sizeof(int64)), (int16)(item.Offset.Ticks / TimeSpan.TicksPerMinute));
        }

        protected override DateTimeOffset To(ref byte location)
        {
            var origin = Endian<int64>.Get(ref location);
            var offset = Endian<int16>.Get(ref Memory.Add(ref location, sizeof(int64)));
            return new DateTimeOffset(origin, new TimeSpan(offset * TimeSpan.TicksPerMinute));
        }
    }
}
