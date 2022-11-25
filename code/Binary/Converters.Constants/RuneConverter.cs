namespace Mikodev.Binary.Converters.Constants;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using System.Text;

internal sealed class RuneConverter : ConstantConverter<Rune, RuneConverter.Functions>
{
    internal readonly struct Functions : IConstantConverterFunctions<Rune>
    {
        public static int Length => sizeof(int);

        public static Rune Decode(ref byte source)
        {
            return new Rune(LittleEndian.Decode<int>(ref source));
        }

        public static void Encode(ref byte target, Rune item)
        {
            LittleEndian.Encode(ref target, item.Value);
        }
    }
}
