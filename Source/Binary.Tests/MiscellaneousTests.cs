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
            static bool Equals(string name, params string[] patterns)
            {
                foreach (var i in patterns)
                    if (name == i || name == (i + "&"))
                        return true;
                return false;
            }

            var names = new[]
            {
                typeof(Allocator).Name,
                typeof(Span<>).Name,
                typeof(Memory<>).Name,
                typeof(ReadOnlySpan<>).Name,
                typeof(ReadOnlyMemory<>).Name,
            };

            var types = typeof(Converter).Assembly.GetTypes();
            var members = types.SelectMany(x => x.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).ToList();
            var methodBases = members.OfType<MethodBase>().ToList();
            var otherMembers = members.Except(methodBases).ToList();
            var parameters = methodBases.SelectMany(x => x.GetParameters()).ToList();

            var AttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
            var inRefParameters = parameters.Where(x => x.GetCustomAttributes().Where(x => x.GetType().FullName == AttributeName).Any()).ToList();
            var isRefParameters = parameters.Where(x => x.ParameterType.IsByRef).ToList();
            var byRefParameters = isRefParameters.Except(inRefParameters).ToList();
            var inRefExpected = inRefParameters.Where(x => typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
            var inRefUnexpected = inRefParameters.Except(inRefExpected).Select(x => x.Member).ToList();

            Assert.NotEmpty(inRefExpected);
            Assert.All(inRefExpected, x => Assert.EndsWith(nameof(IConverter.Decode), x.Member.Name));
            Assert.Empty(inRefUnexpected);

            var converterParameters = parameters.Where(x => x.Member is MethodInfo && typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
            var converterExpectedParameters = converterParameters.Where(x => Equals(x.ParameterType.Name, names)).ToList();
            Assert.All(converterExpectedParameters, x => Assert.True(x.ParameterType.IsByRef));
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
                .Where(x => (x.IsPublic || x.IsNestedPublic) && !(x.IsAbstract && x.IsSealed) && !typeof(Delegate).IsAssignableFrom(x) && !x.IsInterface && x.Namespace == "Mikodev.Binary")
                .ToList();
            Assert.Equal(5, types.Count);
            foreach (var t in types)
            {
                var equalMethod = t.GetMethod("Equals", new[] { typeof(object) });
                var hashMethod = t.GetMethod("GetHashCode", Type.EmptyTypes);
                var stringMethod = t.GetMethod("ToString", Type.EmptyTypes);
                var attributes = new[] { equalMethod, hashMethod, stringMethod }.Select(x => x.GetCustomAttribute<EditorBrowsableAttribute>()).ToList();
                Assert.Equal(3, attributes.Count);
                Assert.All(attributes, Assert.NotNull);
                Assert.True(attributes.All(x => x.State == EditorBrowsableState.Never));
            }
        }

        [Fact(DisplayName = "Public Struct Members All Read Only")]
        public void StructMethods()
        {
            static bool HasReadOnlyAttribute(MemberInfo info) => info.GetCustomAttributes().SingleOrDefault(x => x.GetType().Name == "IsReadOnlyAttribute") != null;

            var types = typeof(Converter).Assembly.GetTypes().Where(x => (x.IsPublic || x.IsNestedPublic) && x.IsValueType).ToList();
            var readonlyTypes = types.Where(HasReadOnlyAttribute).ToList();
            var otherTypes = types.Except(readonlyTypes).ToList();
            var methods = otherTypes.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
            var ignoreMembers = methods.Where(x => x.DeclaringType == typeof(object)).ToList();
            var remainMembers = methods.Except(ignoreMembers).ToList();
            var attributes = remainMembers.Select(x => (x, Flag: HasReadOnlyAttribute(x))).ToList();

            _ = Assert.Single(readonlyTypes);
            _ = Assert.Single(otherTypes);
            Assert.Equal(3, ignoreMembers.Count);
            Assert.All(attributes, x => Assert.True(x.Flag));
        }
    }
}
