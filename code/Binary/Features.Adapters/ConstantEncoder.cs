namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal sealed class ConstantEncoder<E, U> : SpanLikeForwardEncoder<E> where U : struct, IConstantConverterFunctions<E>
{
    public override void Encode(ref Allocator allocator, ReadOnlySpan<E> item)
    {
        Debug.Assert(U.Length >= 1);
        if (item.Length is 0)
            return;
        var length = U.Length;
        ref var target = ref Allocator.Assign(ref allocator, checked(length * item.Length));
        for (var i = 0; i < item.Length; i++)
            U.Encode(ref Unsafe.Add(ref target, length * i), item[i]);
        return;
    }
}
