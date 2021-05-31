using Mikodev.Binary.External.Contexts;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.External
{
    internal sealed class LongDataDictionary : ByteViewDictionary<int>
    {
        private readonly LongDataSlot[] slots;

        public LongDataDictionary(LongDataSlot[] slots)
        {
            Debug.Assert(slots.Any());
            Debug.Assert(slots.All(x => (uint)x.Size <= BinaryObject.LongDataLimits));
            Debug.Assert(slots.Length <= BinaryObject.ItemLimits);
            this.slots = slots;
        }

        public override int GetValue(ref byte source, int length)
        {
            Debug.Assert(length >= 0);
            if ((uint)length > BinaryObject.LongDataLimits)
                return BinaryObject.DataFallback;
            var data = BinaryModule.GetLongData(ref source, length);
            var slots = this.slots;
            for (var i = 0; i < slots.Length; i++)
                if (data == slots[i].Data && length == slots[i].Size)
                    return i;
            return BinaryObject.DataFallback;
        }
    }
}
