namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using System.Diagnostics;
using System.Linq;

internal sealed class LongDataDictionary : ByteViewDictionary<int>
{
    private readonly LongDataSlot[] bits;

    public LongDataDictionary(LongDataSlot[] bits)
    {
        Debug.Assert(bits.Any());
        Debug.Assert(bits.All(x => (uint)(x.Tail & 0xFF) <= BinaryObject.LongDataLimits));
        Debug.Assert(bits.Length <= BinaryObject.ItemLimits);
        this.bits = bits;
    }

    public override int GetValue(ref byte source, int length)
    {
        if ((uint)length > BinaryObject.LongDataLimits)
            return BinaryObject.DataFallback;
        var data = BinaryModule.GetLongData(ref source, length);
        var bits = this.bits;
        for (var i = 0; i < bits.Length; i++)
            if (data.Head == bits[i].Head && data.Tail == bits[i].Tail)
                return i;
        return BinaryObject.DataFallback;
    }
}
