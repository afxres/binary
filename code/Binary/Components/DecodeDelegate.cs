namespace Mikodev.Binary.Components;

using System;

internal delegate T DecodeDelegate<out T>(ref ReadOnlySpan<byte> span);

internal delegate T DecodePassSpanDelegate<out T>(ReadOnlySpan<byte> span);

internal delegate T DecodeReadOnlyDelegate<out T>(in ReadOnlySpan<byte> span);
