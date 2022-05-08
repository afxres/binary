namespace Mikodev.Binary.Features;

using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal interface IRawConverter<T>
{
    static abstract int Length { get; }

    static abstract T Decode(ref byte source);

    static abstract void Encode(ref byte target, T? item);
}
#endif
