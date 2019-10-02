using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeGuidConverter : UnsafeAbstractConverter<Guid, Block16>
    {
        protected override void Of(ref byte location, Guid item) => Endian.SetGuid(ref location, item);

        protected override Guid To(ref byte location) => Endian.GetGuid(ref location);
    }
}
