namespace Mikodev.Binary.Creators.Isolated.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;

internal sealed class DecimalConverter : ConstantConverter<decimal, DecimalConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<decimal>
    {
        public static int Length => sizeof(int) * 4;

        public static decimal Decode(ref byte source)
        {
            const int Limits = 4;
            var buffer = (stackalloc int[Limits]);
            for (var i = 0; i < Limits; i++)
                buffer[i] = LittleEndian.Decode<int>(ref Unsafe.Add(ref source, sizeof(int) * i));
            return new decimal(buffer);
        }

        public static void Encode(ref byte target, decimal item)
        {
            const int Limits = 4;
            var buffer = (stackalloc int[Limits]);
            _ = decimal.GetBits(item, buffer);
            for (var i = 0; i < Limits; i++)
                LittleEndian.Encode(ref Unsafe.Add(ref target, sizeof(int) * i), buffer[i]);
            return;
        }
    }
}
