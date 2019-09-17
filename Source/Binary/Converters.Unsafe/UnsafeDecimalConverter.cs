using Mikodev.Binary.Converters.Unsafe.Abstractions;
using Mikodev.Binary.Internal;

namespace Mikodev.Binary.Converters.Unsafe
{
    internal sealed class UnsafeDecimalConverter : UnsafeConverter<decimal, Block16>
    {
        public override void OfValue(ref byte location, decimal item)
        {
            var source = decimal.GetBits(item);
            Endian<int>.Copy(ref location, ref Memory.AsByte(ref source[0]), sizeof(decimal));
        }

        public override decimal ToValue(ref byte location)
        {
            const int Limits = sizeof(decimal) / sizeof(int);
            var target = new int[Limits];
            Endian<int>.Copy(ref Memory.AsByte(ref target[0]), ref location, sizeof(decimal));
            return new decimal(target);
        }
    }
}
