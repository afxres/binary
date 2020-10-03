namespace Mikodev.Binary.Internal

open System

type UnionDecoder<'T> = delegate of span : byref<ReadOnlySpan<byte>> * mark : byref<int> -> 'T
