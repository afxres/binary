using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Mikodev.Binary.Tests.External
{
    public class LongDataDictionaryTests
    {
        private delegate object CreateDictionary(ImmutableArray<ReadOnlyMemory<byte>> items);

        private delegate int GetValue(ref byte source, int length);

        private static CreateDictionary GetCreateDictionaryDelegate()
        {
            var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BinaryObject");
            Assert.NotNull(type);
            var method = type.GetMethod("CreateLongDataDictionary", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return (CreateDictionary)Delegate.CreateDelegate(typeof(CreateDictionary), method);
        }

        private static GetValue GetGetValueDelegate(object dictionary)
        {
            var method = dictionary.GetType().GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(method);
            return (GetValue)Delegate.CreateDelegate(typeof(GetValue), dictionary, method);
        }

        [Theory(DisplayName = "Dictionary (duplicate key)")]
        [InlineData(new[] { 1, 33, 1024, 33 })]
        [InlineData(new[] { 2, 2, 3, 4 })]
        [InlineData(new[] { 32768, 65535, 65536, 65536 })]
        public void DictionaryDuplicateKey(int[] values)
        {
            var create = GetCreateDictionaryDelegate();
            var arguments = values.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString()))).ToImmutableArray();
            var result = create.Invoke(arguments);
            Assert.Null(result);
        }

        [Fact(DisplayName = "Dictionary (zero character key)")]
        public void DictionaryZeroCharacterKey()
        {
            var create = GetCreateDictionaryDelegate();
            var values = Enumerable.Range(0, 7).Select(x => new string('\0', x)).ToList();
            var arguments = values.Select(x => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(x.ToString()))).ToImmutableArray();
            var result = create.Invoke(arguments);
            var query = GetGetValueDelegate(result);
            Assert.NotNull(result);
            for (var i = 0; i < arguments.Length; i++)
            {
                var buffer = arguments[i].Span;
                var actual = query.Invoke(ref MemoryMarshal.GetReference(buffer), buffer.Length);
                Assert.Equal(i, actual);
            }
        }
    }
}
