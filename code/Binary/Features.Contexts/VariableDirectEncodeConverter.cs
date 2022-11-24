namespace Mikodev.Binary.Features.Contexts;

internal abstract class VariableDirectEncodeConverter<T, U> : VariableConverter<T, U> where U : struct, IVariableDirectEncodeConverterFunctions<T>
{
    private static void EncodeWithLengthPrefixInternal(ref Allocator allocator, T? item)
    {
        var anchor = Allocator.Anchor(ref allocator, sizeof(int));
        U.Encode(ref allocator, item);
        Allocator.FinishAnchor(ref allocator, anchor);
    }

    public override void Encode(ref Allocator allocator, T? item) => U.Encode(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);
}
