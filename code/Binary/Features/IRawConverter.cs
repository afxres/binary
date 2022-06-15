namespace Mikodev.Binary.Features;

#if NET7_0_OR_GREATER
internal interface IRawConverter<T>
{
    static abstract int Length { get; }

    static abstract T Decode(ref byte source);

    static abstract void Encode(ref byte target, T? item);
}
#endif
