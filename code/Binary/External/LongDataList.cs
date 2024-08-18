namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using System.Diagnostics;
using System.Linq;

internal sealed class LongDataList : ByteViewList
{
    private readonly LongDataSlot[] bits;

    public LongDataList(LongDataSlot[] bits)
    {
        Debug.Assert(bits.Length is not 0);
        Debug.Assert(bits.All(x => (uint)(x.Tail & 0xFF) <= BinaryDefine.LongDataListItemBytesLimits));
        Debug.Assert(bits.Length <= BinaryDefine.LongDataListItemCountLimits);
        this.bits = bits;
    }

    public override int Invoke(ref byte source, int length)
    {
        if ((uint)length > BinaryDefine.LongDataListItemBytesLimits)
            return -1;
        var data = BinaryModule.GetLongData(ref source, length);
        var bits = this.bits;
        for (var i = 0; i < bits.Length; i++)
            if (data.Head == bits[i].Head && data.Tail == bits[i].Tail)
                return i;
        return -1;
    }
}
