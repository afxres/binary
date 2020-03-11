using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal static class BinaryNodeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long MakeIndex(ref byte source, int length)
        {
            Debug.Assert(length > 0);
            if (length >= sizeof(long))
                return Unsafe.ReadUnaligned<long>(ref source);
            var index = 0L;
            Unsafe.CopyBlockUnaligned(ref Unsafe.As<long, byte>(ref index), ref source, (uint)length);
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BinaryNode<T> Find<T>(BinaryNode<T>[] nodes, long index)
        {
            Debug.Assert(nodes != null);
            Debug.Assert(nodes.Count(x => x.Index == index) <= 1);
            foreach (var node in nodes)
                if (node.Index == index)
                    return node;
            return null;
        }

        private static BinaryMutableNode<T> FindOrCreate<T>(BinaryMutableNode<T> root, ReadOnlySpan<byte> span)
        {
            var result = root;
            var length = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = MakeIndex(ref Unsafe.Add(ref source, i), length - i);
                var node = result.Nodes.Find(x => x.Index == head);
                if (node is null)
                    result.Nodes.Add(node = new BinaryMutableNode<T> { Index = head });
                result = node;
            }
            return result;
        }

        private static BinaryNode<T> Copy<T>(BinaryMutableNode<T> node)
        {
            return new BinaryNode<T>(node.Nodes.Select(Copy).OrderBy(x => x.Index).ToArray(), node.Index, node.HasValue, node.Value);
        }

        internal static BinaryNode<T> CreateOrDefault<T>(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> enumerable)
        {
            var root = new BinaryMutableNode<T>();
            foreach (var i in enumerable)
            {
                var node = FindOrCreate(root, i.Key.Span);
                if (node.HasValue)
                    return null;
                node.HasValue = true;
                node.Value = i.Value;
            }
            return Copy(root);
        }

        internal static BinaryNode<T> GetOrDefault<T>(BinaryNode<T> root, ref byte source, int length)
        {
            Debug.Assert(length >= 0);
            var result = root;
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = MakeIndex(ref Unsafe.Add(ref source, i), length - i);
                var node = Find(result.Nodes, head);
                if (node is null)
                    return null;
                result = node;
            }
            return result.HasValue ? result : null;
        }
    }
}
