namespace Mikodev.Binary.Features;

#if NET6_0
[System.Runtime.Versioning.RequiresPreviewFeatures]
#endif
internal interface IRawConverter<T>
{
    static abstract int Length { get; }

    static abstract T Decode(ref byte source);

    static abstract void Encode(ref byte target, T? item);
}
