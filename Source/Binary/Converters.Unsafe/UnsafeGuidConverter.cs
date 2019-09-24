using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeGuidConverter : UnsafeConverter<Guid, Block16>
    {
        public override void OfValue(ref byte location, Guid item) => Endian.SetGuid(ref location, item);

        public override Guid ToValue(ref byte location) => Endian.GetGuid(ref location);
    }
}
