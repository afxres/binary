using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeDecimalConverter : UnsafeAbstractConverter<decimal, Block16>
    {
        protected override void Of(ref byte location, decimal item)
        {
            var source = decimal.GetBits(item);
            Endian<int>.Copy(ref location, ref Memory.AsByte(ref source[0]), sizeof(decimal));
        }

        protected override decimal To(ref byte location)
        {
            const int Limits = sizeof(decimal) / sizeof(int);
            var target = new int[Limits];
            Endian<int>.Copy(ref Memory.AsByte(ref target[0]), ref location, sizeof(decimal));
            return new decimal(target);
        }
    }
}
