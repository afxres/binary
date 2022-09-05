namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System.Text;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal readonly struct RuneRawConverter : IRawConverter<Rune>
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
