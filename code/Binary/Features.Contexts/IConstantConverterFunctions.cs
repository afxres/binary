namespace Mikodev.Binary.Features.Contexts;

internal interface IConstantConverterFunctions<T>
{
    static abstract int Length { get; }

    static abstract T Decode(ref byte source);

    static abstract void Encode(ref byte target, T? item);
}
