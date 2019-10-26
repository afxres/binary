using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class MiscellaneousTests
    {
        [Fact(DisplayName = "Public Class Methods With ByRef Type")]
        public void Argument()
        {
            static bool StartsWith(string name, params string[] patterns)
            {
                foreach (var i in patterns)
                    if (name.StartsWith(i))
                        return true;
                return false;
            }

            var names = new[]
            {
                typeof(Allocator).Name,
                typeof(ArraySegment<>).Name,
                typeof(Span<>).Name,
                typeof(Memory<>).Name,
                typeof(ReadOnlySpan<>).Name,
                typeof(ReadOnlyMemory<>).Name,
            };

            var types = typeof(Converter).Assembly.GetTypes().Where(x => x.IsPublic);
            var methods = types.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).ToList();
            var parameters = methods.SelectMany(x => x.GetParameters()).ToList();
            var source = parameters.Where(x => StartsWith(x.ParameterType.Name, names)).ToList();
            var failed = source.Where(x => !x.ParameterType.IsByRef).Select(x => x.Member).Cast<MethodInfo>().Select(x => x.ReflectedType).Distinct();
            Assert.Empty(failed);
            Assert.NotEmpty(source);
        }

        [Fact(DisplayName = "Types With Sealed Modifier")]
        public void Sealed()
        {
            var types = typeof(Converter).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsValueType || type.IsAbstract || type.IsInterface)
                    continue;
                Assert.True(type.IsSealed);
            }
        }

        [Fact(DisplayName = "Multi Threads (Thread Static)")]
        public async Task MultiThreadsTestAsync()
        {
            var generator = Generator.CreateDefault();
            const int count = 16;
            const int times = 1 << 10;
            var funcs = Enumerable.Range(0, count).Select(x => new Action(() =>
            {
                var model = Enumerable.Range(0, 256).Select(_ => Guid.NewGuid().ToString()).ToArray();
                for (var i = 0; i < times; i++)
                {
                    var bytes = generator.Encode(model);
                    var value = generator.Decode(bytes, anonymous: model);
                    Assert.Equal<string>(model, value);
                }
            })).ToList();
            var tasks = funcs.Select(Task.Run).ToList();
            await Task.WhenAll(tasks);
        }

        [Fact(DisplayName = "Public Types (Namespace)")]
        public void PublicTypes()
        {
            var array = new[]
            {
                "Mikodev.Binary",
                "Mikodev.Binary.Attributes",
            };
            var types = typeof(Converter).Assembly.GetTypes().ToHashSet();
            var alpha = types.Where(x => array.Contains(x.Namespace) && !x.IsNested).ToHashSet();
            var bravo = types.Where(x => x.Name.Any(c => c == '<' || c == '>')).ToHashSet();
            var delta = types.Except(alpha).Except(bravo).ToHashSet();
            Assert.Equal(types.Count, alpha.Count + bravo.Count + delta.Count);
            Assert.Equal(types, alpha.Union(bravo).Union(delta).ToHashSet());
            Assert.True(alpha.All(x => x.IsPublic));
            Assert.True(delta.All(x => !x.IsPublic));
        }

        [Fact(DisplayName = "Public Class Object Methods All Invisible")]
        public void PublicObjectMethods()
        {
            var types = typeof(Converter).Assembly.GetTypes()
                .Where(x => x.IsPublic && !(x.IsAbstract && x.IsSealed) && !x.IsInterface && x.Namespace == "Mikodev.Binary")
                .ToList();
            Assert.Equal(4, types.Count);
            foreach (var t in types)
            {
                var equalMethod = t.GetMethod("Equals", new[] { typeof(object) });
                var hashMethod = t.GetMethod("GetHashCode", Type.EmptyTypes);
                var stringMethod = t.GetMethod("ToString", Type.EmptyTypes);
                var attributes = new[] { equalMethod, hashMethod, stringMethod }.Select(x => x.GetCustomAttribute<EditorBrowsableAttribute>()).ToList();
                Assert.Equal(3, attributes.Count);
                Assert.True(attributes.All(x => x.State == EditorBrowsableState.Never));
            }
        }
    }
}
