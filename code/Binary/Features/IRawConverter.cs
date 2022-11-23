namespace Mikodev.Binary.Features;

internal interface IRawConverter<T>
{
    static abstract int Length { get; }

    static abstract T Decode(ref byte source);

    static abstract void Encode(ref byte target, T? item);
}
