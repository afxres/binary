namespace Mikodev.Binary.Components;

using System;

public delegate T CollectionDecoder<out T>(scoped ReadOnlySpan<byte> span);
