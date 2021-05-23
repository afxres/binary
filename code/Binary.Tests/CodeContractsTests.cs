using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class CodeContractsTests
    {
        [Fact(DisplayName = "Public Class Methods With ByRef Type")]
        public void Argument()
        {
            static bool Equals(string name, params string[] patterns)
            {
                return patterns.Any(i => name == i || name == (i + "&"));
            }

            var names = new[]
            {
                typeof(Allocator).Name,
                typeof(Span<>).Name,
                typeof(Memory<>).Name,
                typeof(ReadOnlySpan<>).Name,
                typeof(ReadOnlyMemory<>).Name,
            };

            var types = typeof(IConverter).Assembly.GetTypes();
            var members = types.SelectMany(x => x.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).ToList();
            var methodBases = members.OfType<MethodBase>().ToList();
            var parameters = methodBases.SelectMany(x => x.GetParameters()).ToList();

            var AttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
            var inRefParameters = parameters.Where(x => x.GetCustomAttributes().Any(a => a.GetType().FullName == AttributeName)).ToList();
            var inRefExpected = inRefParameters.Where(x => typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
            var inRefUnexpected = inRefParameters.Except(inRefExpected).Select(x => x.Member).ToList();

            Assert.NotEmpty(inRefExpected);
            Assert.All(inRefExpected, x => Assert.EndsWith(nameof(IConverter.Decode), x.Member.Name));
            Assert.NotEmpty(inRefUnexpected);
            Assert.All(inRefUnexpected, x => Assert.True(x.DeclaringType.IsSubclassOf(typeof(Delegate))));

            var converterParameters = parameters.Where(x => x.Member is MethodInfo && typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
            var converterExpectedParameters = converterParameters.Where(x => !x.Member.Name.StartsWith("Throw") && Equals(x.ParameterType.Name, names)).ToList();
            var ignoredParameters = converterExpectedParameters.Where(x => !x.ParameterType.IsByRef).ToList();
            Assert.All(ignoredParameters, x => Assert.Equal("DecodeInternal", x.Member.Name));
        }

        [Fact(DisplayName = "Types With Sealed Modifier")]
        public void Sealed()
        {
            var types = typeof(IConverter).Assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsValueType || type.IsAbstract || type.IsInterface)
                    continue;
                Assert.True(type.IsSealed);
            }
        }

        [Fact(DisplayName = "Public Types (namespace)")]
        public void PublicTypes()
        {
            var array = new[]
            {
                "Mikodev.Binary",
                "Mikodev.Binary.Attributes",
            };
            var types = new HashSet<Type>(typeof(IConverter).Assembly.GetTypes());
            var alpha = new HashSet<Type>(types.Where(x => array.Contains(x.Namespace) && !x.IsNested));
            var bravo = new HashSet<Type>(types.Where(x => x.Name.Any(c => c is '<' or '>' or '-')));
            var delta = new HashSet<Type>(types.Except(alpha).Except(bravo));
            var deltaMembers = delta.SelectMany(x => x.GetMethods(BindingFlags.Static | BindingFlags.Public)).ToList();
            Assert.Equal(types.Count, alpha.Count + bravo.Count + delta.Count);
            Assert.Equal(types, new HashSet<Type>(alpha.Union(bravo).Union(delta)));
            Assert.True(alpha.All(x => x.IsPublic));
            Assert.True(delta.All(x => !x.IsPublic));
            Assert.Empty(deltaMembers);
        }

        [Fact(DisplayName = "Public Class Object Methods All Invisible")]
        public void PublicObjectMethods()
        {
            var types = typeof(IConverter).Assembly.GetTypes()
                .Where(x => (x.IsPublic || x.IsNestedPublic) && !(x.IsAbstract && x.IsSealed) && !typeof(Delegate).IsAssignableFrom(x) && !x.IsInterface && x.Namespace == "Mikodev.Binary")
                .ToList();
            Assert.Equal(3, types.Count);
            foreach (var t in types)
            {
                var equalMethod = t.GetMethodNotNull("Equals", new[] { typeof(object) });
                var hashMethod = t.GetMethodNotNull("GetHashCode", Type.EmptyTypes);
                var stringMethod = t.GetMethodNotNull("ToString", Type.EmptyTypes);
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

            var types = typeof(IConverter).Assembly.GetTypes().Where(x => (x.IsPublic || x.IsNestedPublic) && x.IsValueType).ToList();
            var readonlyTypes = types.Where(HasReadOnlyAttribute).ToList();
            var otherTypes = types.Except(readonlyTypes).ToList();
            var methods = otherTypes.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
            var ignoreMembers = methods.Where(x => x.DeclaringType == typeof(object)).ToList();
            var remainMembers = methods.Except(ignoreMembers).ToList();
            var attributes = remainMembers.Select(x => (x, Flag: HasReadOnlyAttribute(x))).ToList();

            _ = Assert.Single(otherTypes);
            Assert.Empty(readonlyTypes);
            Assert.Equal(3, ignoreMembers.Count);
            Assert.All(attributes, x => Assert.True(x.Flag));
        }

        [Fact(DisplayName = "Debugger Display")]
        public void DebuggerDisplay()
        {
            var types = typeof(IConverter).Assembly.GetTypes();
            var attributes = types.Select(x => x.GetCustomAttribute<DebuggerDisplayAttribute>()).ToList();
            Assert.All(attributes, Assert.Null);
        }

        [Fact(DisplayName = "Public Methods With Byte Array Parameter")]
        public void ByteArray()
        {
            var types = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsPublic).ToList();
            var members = types.SelectMany(x => x.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)).ToList();
            var methodBases = members.OfType<MethodBase>().ToList();
            Assert.NotEmpty(methodBases);
            Assert.NotEmpty(methodBases.OfType<ConstructorInfo>());

            var parameters = methodBases.SelectMany(x => x.GetParameters()).ToList();
            var byteArrayParameters = parameters.Where(x => x.ParameterType == typeof(byte[])).ToList();
            Assert.Equal(5, byteArrayParameters.Count);
            Assert.All(byteArrayParameters, x => Assert.Equal("Decode", x.Member.Name));
        }

        [Fact(DisplayName = "Is Ref Struct")]
        public void IsByRefLike()
        {
            var attributeName = "System.Runtime.CompilerServices.IsByRefLikeAttribute";
            var types = typeof(IConverter).Assembly.GetTypes();
            var byRefTypes = (from t in types let attributes = t.GetCustomAttributes() where attributes.Any(x => x.GetType().FullName == attributeName) select t).ToList();
            Assert.Contains(typeof(Allocator), byRefTypes);
        }

        [Fact(DisplayName = "Method With 'DoesNotReturnAttribute'")]
        public void NoReturn()
        {
            const string AttributeName = "System.Diagnostics.CodeAnalysis.DoesNotReturnAttribute";
            var types = typeof(IConverter).Assembly.GetTypes();
            var methods = types.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
            var attributedMethods = methods.Where(x => x.GetCustomAttributes().SingleOrDefault(a => a.GetType().FullName == AttributeName) != null).ToList();
            Assert.All(attributedMethods, x => Assert.True((x.DeclaringType.Name == "ThrowHelper" && x.Name.StartsWith("Throw")) || x.Name.StartsWith("Except")));

            var expectedMethods = methods.Where(x => x.Name.Contains("Throw") || x.Name.Contains("Except")).ToList();
            Assert.Equal(new HashSet<MethodInfo>(attributedMethods), new HashSet<MethodInfo>(expectedMethods));

            var misspelledMethods = methods.Where(x => x.Name.ToUpperInvariant().Contains("Expect".ToUpperInvariant())).ToList();
            Assert.Empty(misspelledMethods);
        }

        [Fact(DisplayName = "Generic Converter Constructor (protected)")]
        public void ConverterGenericAccessLevel()
        {
            var constructors = typeof(Converter<int>).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.Equal(2, constructors.Length);
            foreach (var constructor in constructors)
            {
                // protected, not internal
                Assert.True(constructor.IsFamily);
                Assert.False(constructor.IsAssembly);
            }
        }

        [Fact(DisplayName = "Internal Type Instance Member Access")]
        public void InternalTypeInstanceMemberAccessLevel()
        {
            var types = typeof(IConverter).Assembly.GetTypes();
            var array = types.Where(x => x.IsPublic is false && x.IsSubclassOf(typeof(Delegate)) is false && x.IsEnum is false).ToList();
            var filter = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var t in array)
            {
                var fields = t.GetFields(filter);
                var methods = t.GetMethods(filter);
                var constructors = t.GetConstructors(filter);
                var properties = t.GetProperties(filter);
                Assert.All(properties, x => Assert.True(x.GetGetMethod().IsPublic));
                Assert.All(fields, x => Assert.True(x.IsPublic || x.IsPrivate));
                Assert.All(methods, x => Assert.True(x.Name.Contains("<") || x.DeclaringType == typeof(object) || x.IsPublic || x.IsPrivate));
                Assert.All(constructors, x => Assert.True((x.DeclaringType.IsAbstract && x.GetParameters().Length is 0) || x.IsPublic || x.IsPrivate));
            }
        }
    }
}
