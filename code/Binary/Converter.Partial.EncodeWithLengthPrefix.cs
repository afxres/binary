namespace Mikodev.Binary;

using Mikodev.Binary.Internal;

public abstract partial class Converter<T>
{
    private void EncodeWithLengthPrefixConstant(ref Allocator allocator, T item)
    {
        var length = this.length;
        var numberLength = NumberHelper.EncodeLength((uint)length);
        NumberHelper.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)length, numberLength);
        Encode(ref allocator, item);
    }

    private void EncodeWithLengthPrefixVariable(ref Allocator allocator, T item)
    {
        var anchor = Allocator.Anchor(ref allocator, sizeof(int));
        Encode(ref allocator, item);
        Allocator.FinishAnchor(ref allocator, anchor);
    }
}
