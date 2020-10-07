using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class ExternalTests
    {
        private delegate object NodeOrNull<T>(object root, ref byte source, int length);

        private delegate object MakeOrNull<T>(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> enumerable);

        private delegate object Node<T>(IReadOnlyList<object> values, long header, bool exists, T intent);

        private delegate void NodeData<T>(object node, out long header, out bool exists, out T intent, out IReadOnlyList<object> values);

        private sealed class BinaryHelper<T>
        {
            public readonly Node<T> Node;

            public readonly NodeData<T> NodeData;

            public readonly NodeOrNull<T> NodeOrNull;

            public readonly MakeOrNull<T> MakeOrNull;

            public readonly IReadOnlyList<object> EmptyNodes;

            public BinaryHelper()
            {
                var nodeType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == "Node`1").MakeGenericType(typeof(T));
                Node = CompileNode(nodeType);
                NodeData = CompileNodeData(nodeType);
                NodeOrNull = CompileNodeOrNull(nodeType);
                MakeOrNull = CompileMakeOrNull();
                EmptyNodes = (IReadOnlyList<object>)Array.CreateInstance(nodeType, 0);
            }

            private Node<T> CompileNode(Type nodeType)
            {
                var header = Expression.Parameter(typeof(long));
                var exists = Expression.Parameter(typeof(bool));
                var intent = Expression.Parameter(typeof(T));
                var values = Expression.Parameter(typeof(IReadOnlyList<object>));
                var constructor = nodeType.GetConstructors().Single();
                var lambda = Expression.Lambda<Node<T>>(Expression.New(constructor, Expression.Convert(values, nodeType.MakeArrayType()), header, exists, intent), values, header, exists, intent);
                return lambda.Compile();
            }

            private NodeData<T> CompileNodeData(Type nodeType)
            {
                var node = Expression.Parameter(typeof(object));
                var header = Expression.Parameter(typeof(long).MakeByRefType());
                var exists = Expression.Parameter(typeof(bool).MakeByRefType());
                var intent = Expression.Parameter(typeof(T).MakeByRefType());
                var values = Expression.Parameter(typeof(IReadOnlyList<object>).MakeByRefType());

                BinaryExpression MakeAssign(ParameterExpression leftValue, string fieldName)
                {
                    var fieldInfo = nodeType.GetField(fieldName);
                    var cast = Expression.Convert(node, nodeType);
                    var info = Expression.Field(cast, fieldInfo);
                    return Expression.Assign(leftValue, info);
                }

                var expressions = new Expression[]
                {
                    MakeAssign(header, "Header"),
                    MakeAssign(exists, "Exists"),
                    MakeAssign(intent, "Intent"),
                    MakeAssign(values, "Values"),
                };

                var lambda = Expression.Lambda<NodeData<T>>(Expression.Block(expressions), node, header, exists, intent, values);
                return lambda.Compile();
            }

            private NodeOrNull<T> CompileNodeOrNull(Type nodeType)
            {
                var root = Expression.Parameter(typeof(object), "root");
                var source = Expression.Parameter(typeof(byte).MakeByRefType(), "source");
                var length = Expression.Parameter(typeof(int), "length");
                var helperType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == "NodeTreeHelper");
                var method = helperType.GetMethod("NodeOrNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(T));
                var lambda = Expression.Lambda<NodeOrNull<T>>(Expression.Call(method, Expression.Convert(root, nodeType), source, length), root, source, length);
                return lambda.Compile();
            }

            private MakeOrNull<T> CompileMakeOrNull()
            {
                var helperType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == "NodeTreeHelper");
                var method = helperType.GetMethod("MakeOrNull", BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeof(T));
                return (MakeOrNull<T>)Delegate.CreateDelegate(typeof(MakeOrNull<T>), method);
            }
        }

        private void AssertNode<T>(BinaryHelper<T> helper, object node, bool exists, T intent, int length, string view)
        {
            helper.NodeData.Invoke(node, out var _, out var actualExists, out var actualIntent, out var actualValues);
            Assert.Equal(view, node.ToString());
            Assert.Equal(exists, actualExists);
            Assert.Equal(intent, actualIntent);
            Assert.Equal(length, actualValues.Count);
        }

        [Fact(DisplayName = "Node Debug View (no value, ASCII printable characters)")]
        public void NodeDebugViewPrintable()
        {
            var helper = new BinaryHelper<string>();
            for (var i = 0; i < 8; i++)
            {
                for (var k = (int)byte.MinValue; k < byte.MaxValue; k++)
                {
                    var node = helper.Node.Invoke(helper.EmptyNodes, (long)((ulong)k << (i * 8)), false, null);
                    var text = k is >= 32 and <= 126
                        ? Enumerable.Repeat('.', i).Concat(new[] { (char)k }).Concat(Enumerable.Repeat('.', 8 - 1 - i)).Aggregate(new StringBuilder(), (a, b) => a.Append(b)).ToString()
                        : "........";
                    var view = node.ToString();
                    Assert.Equal($"Node(Header: '{text}', Values: 0)", view);
                }
            }
        }

        [Theory(DisplayName = "Node Debug View (has value)")]
        [InlineData(0x4142434445464748, "Brown Fox", "HGFEDCBA")]
        [InlineData(0x002064627E3D3F34, -3.14, "4?=~bd .")]
        public void NodeDebugView<T>(long header, T intent, string headerView)
        {
            var helper = new BinaryHelper<T>();
            var node = helper.Node.Invoke(helper.EmptyNodes, header, true, intent);
            var view = node.ToString();
            Assert.Equal($"Node(Header: '{headerView}', Values: 0, Intent: '{intent}')", view);
        }

        [Theory(DisplayName = "Node Tree (single node)")]
        [InlineData("0")]
        [InlineData(" _ =")]
        [InlineData("7355608")]
        [InlineData("External")]
        public void NodeTreeSingle(string text)
        {
            var helper = new BinaryHelper<string>();
            var encoding = Encoding.UTF8;
            var bytes = encoding.GetBytes(text);
            var enumerable = new[] { new KeyValuePair<ReadOnlyMemory<byte>, string>(bytes, text) };
            var root = helper.MakeOrNull.Invoke(enumerable);
            AssertNode(helper, root, false, default, 1, "Node(Header: '........', Values: 1)");
            helper.NodeData.Invoke(root, out _, out _, out _, out var values);
            var node = Assert.Single(values);
            var view = $"Node(Header: '{new string(text.Concat(Enumerable.Repeat('.', 8)).Take(8).ToArray())}', Values: 0, Intent: '{text}')";
            AssertNode(helper, node, true, text, 0, view);
            var result = helper.NodeOrNull(root, ref bytes[0], bytes.Length);
            Assert.True(ReferenceEquals(node, result));
        }

        [Theory(DisplayName = "Node Tree (single node, empty key)")]
        [InlineData(2.71)]
        [InlineData("alpha")]
        public void NodeTreeSingleEmptyKey<T>(T intent)
        {
            var helper = new BinaryHelper<T>();
            var enumerable = new[] { new KeyValuePair<ReadOnlyMemory<byte>, T>(default, intent) };
            var root = helper.MakeOrNull.Invoke(enumerable);
            AssertNode(helper, root, true, intent, 0, $"Node(Header: '........', Values: 0, Intent: '{intent}')");
        }

        public class NodeTreeData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var alpha = new string[] { string.Empty, "Alpha", "LongLongLongName" };
                var bravo = new string[] { "x", "The quick brown fox jumps over the lazy dog", string.Empty, "9876543", };
                yield return new object[] { alpha };
                yield return new object[] { bravo };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "Node Tree")]
        [ClassData(typeof(NodeTreeData))]
        public void NodeTree(IReadOnlyList<string> list)
        {
            var helper = new BinaryHelper<string>();
            var encoding = Encoding.UTF8;
            var memories = list.ToDictionary(x => x, x => encoding.GetBytes(x));
            var enumerable = memories.Select(x => new KeyValuePair<ReadOnlyMemory<byte>, string>(x.Value, x.Key)).ToList();
            var root = helper.MakeOrNull.Invoke(enumerable);
            foreach (var i in memories)
            {
                var span = new ReadOnlySpan<byte>(i.Value);
                var node = helper.NodeOrNull.Invoke(root, ref MemoryMarshal.GetReference(span), span.Length);
                Assert.NotNull(node);
                helper.NodeData.Invoke(node, out _, out var exists, out var intent, out _);
                Assert.True(exists);
                Assert.Equal(i.Key, intent);
            }
        }

        public class NodeTreeNotFoundData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var alpha = new string[] { string.Empty, "Alpha", "LongLongLongName" };
                var bravo = new string[] { "x", "The quick brown fox jumps over the lazy dog", string.Empty, "9876543", };
                yield return new object[] { alpha, "alpha" };
                yield return new object[] { bravo, "The quic" };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "Node Tree Not Found")]
        [ClassData(typeof(NodeTreeNotFoundData))]
        public void NodeTreeNotFound(IReadOnlyList<string> list, string key)
        {
            var helper = new BinaryHelper<string>();
            var encoding = Encoding.UTF8;
            var memories = list.ToDictionary(x => x, x => encoding.GetBytes(x));
            var enumerable = memories.Select(x => new KeyValuePair<ReadOnlyMemory<byte>, string>(x.Value, x.Key)).ToList();
            var root = helper.MakeOrNull.Invoke(enumerable);
            var source = encoding.GetBytes(key).AsSpan();
            var result = helper.NodeOrNull(root, ref MemoryMarshal.GetReference(source), source.Length);
            Assert.Null(result);
        }

        public class NodeTreeSame : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                var alpha = new string[] { "alpha", "alpha" };
                var bravo = new string[] { "LongLongName", "NameLong", "LongLongName" };
                yield return new object[] { alpha };
                yield return new object[] { bravo };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory(DisplayName = "Node Tree (create with duplicate key)")]
        [ClassData(typeof(NodeTreeSame))]
        public void NodeTreeSameKey(IReadOnlyList<string> list)
        {
            var helper = new BinaryHelper<string>();
            var encoding = Encoding.UTF8;
            var enumerable = list.Select(x => new KeyValuePair<ReadOnlyMemory<byte>, string>(encoding.GetBytes(x), x)).ToList();
            Assert.NotEmpty(enumerable);
            var root = helper.MakeOrNull.Invoke(enumerable);
            Assert.Null(root);
        }

        [Fact(DisplayName = "Node Tree (create, access, large data)")]
        public void LargeDataTests()
        {
            var helper = new BinaryHelper<string>();
            var names = typeof(object).Assembly.GetTypes().Where(x => x.IsPublic).Select(x => x.Name).Distinct().ToList();
            var encoding = Encoding.UTF8;
            var enumerable = names.Select(x => new KeyValuePair<ReadOnlyMemory<byte>, string>(encoding.GetBytes(x), x)).ToList();
            Assert.NotEmpty(enumerable);
            var root = helper.MakeOrNull(enumerable);
            Assert.NotNull(root);
            foreach (var i in enumerable)
            {
                var span = i.Key.Span;
                var node = helper.NodeOrNull.Invoke(root, ref MemoryMarshal.GetReference(span), span.Length);
                Assert.NotNull(node);
                helper.NodeData.Invoke(node, out _, out var exists, out var intent, out _);
                Assert.True(exists);
                Assert.Equal(i.Value, intent);
            }
        }

        [Fact(DisplayName = "Node Tree (children order)")]
        public void OrderTests()
        {
            var helper = new BinaryHelper<string>();

            long Selector(object item)
            {
                helper.NodeData.Invoke(item, out var header, out _, out _, out _);
                return header;
            }

            var names = typeof(object).Assembly.GetTypes().Where(x => x.IsPublic).Select(x => x.Name).Distinct().ToList();
            var encoding = Encoding.UTF8;
            var enumerable = names.Select(x => new KeyValuePair<ReadOnlyMemory<byte>, string>(encoding.GetBytes(x), x)).ToList();
            Assert.NotEmpty(enumerable);
            var root = helper.MakeOrNull(enumerable);
            Assert.NotNull(root);
            helper.NodeData.Invoke(root, out _, out _, out _, out var values);

            var result = values.OrderBy(Selector).ToArray();
            for (var i = 0; i < result.Length; i++)
            {
                var origin = values[i];
                var actual = result[i];
                Assert.True(ReferenceEquals(origin, actual));
            }
        }
    }
}
