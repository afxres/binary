namespace Mikodev.Binary.Features.Contexts;

internal abstract class VariableWriterEncodeConverter<T, U> : VariableConverter<T, U> where U : struct, IVariableWriterEncodeConverterFunctions<T>
{
    private readonly AllocatorWriter<T?> writer;

    public VariableWriterEncodeConverter() => this.writer = U.Encode;

    public override void Encode(ref Allocator allocator, T? item) => Allocator.Append(ref allocator, U.GetMaxLength(item), item, this.writer);

    public override void EncodeAuto(ref Allocator allocator, T? item) => Allocator.AppendWithLengthPrefix(ref allocator, U.GetMaxLength(item), item, this.writer);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => Allocator.AppendWithLengthPrefix(ref allocator, U.GetMaxLength(item), item, this.writer);
}
