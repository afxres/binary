using Mikodev.Binary.Tests.Internal;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class CollectionReflectionTests
    {
        private class FakeList<T>
        {
            public T Data;
        }

        [Theory(DisplayName = "Get List Builder (fake invalid list type)")]
        [InlineData(typeof(FakeList<int>), typeof(int))]
        public void FakeListType(Type type, Type itemType)
        {
            var method = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "FallbackSequentialMethods").GetMethodNotNull("CreateListBuilder", BindingFlags.Static | BindingFlags.NonPublic);
            var source = (Func<Type, Type, object>)Delegate.CreateDelegate(typeof(Func<Type, Type, object>), method);
            var result = source.Invoke(type, itemType);
            Assert.Null(result);
        }
    }
}
