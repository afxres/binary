﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
            ref var target = ref Unsafe.As<long, byte>(ref index);
            for (var i = 0; i < length; i++)
                Unsafe.Add(ref target, i) = Unsafe.Add(ref source, i);
            return index;
        }

        private static BinaryNode<T> GetOrCreate<T>(BinaryNode<T> root, ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryMarshal.GetReference(span);
            var length = span.Length;
            var result = root;
            for (var i = 0; i < length; i += sizeof(long))
            {
                var head = GetIndex(ref Unsafe.Add(ref source, i), length - i);
                var body = result.Nodes?.SingleOrDefault(x => x.Index == head);
                if (body is null)
                {
                    body = new BinaryNode<T> { Index = head };
                    result.Nodes = new List<BinaryNode<T>>(result.Nodes ?? Array.Empty<BinaryNode<T>>()) { body }.ToArray();
                }
                result = body;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BinaryNode<T> Get<T>(BinaryNode<T>[] nodes, long index)
        {
            if (nodes is null)
                return null;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.Index != index)
                    continue;
                return node;
            }
            return null;
        }

        internal static BinaryNode<T> Create<T>(Encoding encoding, IReadOnlyDictionary<string, T> enumerable)
        {
            var root = new BinaryNode<T>();
            foreach (var i in enumerable)
            {
                var node = GetOrCreate(root, encoding.GetBytes(i.Key));
                if (node.HasValue)
                    throw new ArgumentException("Invalid or duplicate key detected!");
                node.HasValue = true;
                node.Value = i.Value;
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
                var node = Get(result.Nodes, head);
                if (node is null)
                    return null;
                result = node;
            }
            return result.HasValue ? result : null;
        }
    }
}
