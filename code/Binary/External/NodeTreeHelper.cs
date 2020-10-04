using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal static class NodeTreeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long MakeHeader(ref byte source, int length)
        {
            Debug.Assert(length > 0);
            if (length >= 8)
                return MemoryHelper.DecodeLittleEndian<long>(ref source);
            var result = (length & 4) is 0 ? 0 : (ulong)(uint)MemoryHelper.DecodeLittleEndian<int>(ref Unsafe.Add(ref source, length & 3));
            for (var i = (length & 3) - 1; i >= 0; i--)
                result = (result << 8) | MemoryHelper.DecodeNativeEndian<byte>(ref Unsafe.Add(ref source, i));
            return (long)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Node<T> Find<T>(Node<T>[] values, long header)
        {
            Debug.Assert(values is not null);
            Debug.Assert(values.Count(x => x.Header == header) <= 1);
            foreach (var node in values)
                if (node.Header == header)
                    return node;
            return null;
        }

        private static NodeTreeVariable<T> FindOrMake<T>(NodeTreeVariable<T> root, ReadOnlySpan<byte> span)
        {
            var result = root;
            var length = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = MakeHeader(ref Unsafe.Add(ref source, i), length - i);
                var node = result.Values.Find(x => x.Header == head);
                if (node is null)
                    result.Values.Add(node = new NodeTreeVariable<T> { Header = head });
                result = node;
            }
            return result;
        }

        private static Node<T> Copy<T>(NodeTreeVariable<T> node)
        {
            return new Node<T>(node.Values.Select(Copy).OrderBy(x => x.Header).ToArray(), node.Header, node.Exists, node.Intent);
        }

        internal static Node<T> MakeOrNull<T>(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> enumerable)
        {
            var root = new NodeTreeVariable<T>();
            foreach (var i in enumerable)
            {
                var node = FindOrMake(root, i.Key.Span);
                if (node.Exists)
                    return null;
                node.Exists = true;
                node.Intent = i.Value;
            }
            return Copy(root);
        }

        internal static Node<T> NodeOrNull<T>(Node<T> root, ref byte source, int length)
        {
            Debug.Assert(length >= 0);
            var result = root;
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = MakeHeader(ref Unsafe.Add(ref source, i), length - i);
                var node = Find(result.Values, head);
                if (node is null)
                    return null;
                result = node;
            }
            return result.Exists ? result : null;
        }
    }
}
