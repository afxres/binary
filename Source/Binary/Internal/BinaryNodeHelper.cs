using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal static class BinaryNodeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long GetIndex(ref byte source, int length)
        {
            Debug.Assert(length > 0);
            if (length >= sizeof(long))
                return Unsafe.As<byte, long>(ref source);
            var index = 0L;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<long, byte>(ref index), ref source, (uint)length);
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BinaryNode<T> Get<T>(ReadOnlySpan<BinaryNode<T>> nodes, long index)
        {
            Debug.Assert(nodes.ToArray().Count(x => x.Index == index) <= 1);
            foreach (var node in nodes)
                if (node.Index == index)
                    return node;
            return null;
        }

        private static BinaryNode<T> GetOrCreate<T>(BinaryNode<T> root, ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryMarshal.GetReference(span);
            var length = span.Length;
            var result = root;
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = GetIndex(ref Unsafe.Add(ref source, i), length - i);
                var node = Get(new ReadOnlySpan<BinaryNode<T>>(result.Nodes), head);
                if (node is null)
                {
                    node = new BinaryNode<T> { Index = head };
                    var list = new List<BinaryNode<T>>(result.Nodes ?? Array.Empty<BinaryNode<T>>()) { node };
                    result.Nodes = list.ToArray();
                }
                result = node;
            }
            return result;
        }

        internal static BinaryNode<T> CreateOrDefault<T>(IReadOnlyCollection<(ReadOnlyMemory<byte>, T)> enumerable)
        {
            var root = new BinaryNode<T>();
            foreach (var (key, value) in enumerable)
            {
                var node = GetOrCreate(root, key.Span);
                if (node.HasValue)
                    return null;
                node.HasValue = true;
                node.Value = value;
            }
            return root;
        }

        internal static BinaryNode<T> GetOrDefault<T>(BinaryNode<T> root, ref byte source, int length)
        {
            Debug.Assert(length >= 0);
            var result = root;
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = GetIndex(ref Unsafe.Add(ref source, i), length - i);
                var node = Get(new ReadOnlySpan<BinaryNode<T>>(result.Nodes), head);
                if (node is null)
                    return null;
                result = node;
            }
            return result.HasValue ? result : null;
        }
    }
}
