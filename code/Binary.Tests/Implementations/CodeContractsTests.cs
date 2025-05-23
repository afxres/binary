﻿namespace Mikodev.Binary.Tests.Implementations;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

public class CodeContractsTests
{
    [Fact(DisplayName = "Public Class Method With ByRef Type")]
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

        const string AttributeName = "System.Runtime.CompilerServices.IsReadOnlyAttribute";
        var inRefParameters = parameters.Where(x => x.GetCustomAttributes()?.Any(a => a.GetType().FullName == AttributeName) is true).ToList();
        var inRefExpected = inRefParameters.Where(x => typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
        var inRefUnexpected = inRefParameters.Except(inRefExpected).Select(x => x.Member).ToList();

        Assert.NotEmpty(inRefExpected);
        Assert.All(inRefExpected, x => Assert.Contains("Decode", x.Member.Name));
        Assert.NotEmpty(inRefUnexpected);
        Assert.All(inRefUnexpected, x => Assert.True(x.DeclaringType is { } declaring && (declaring.Name.Contains('<') || declaring.Name.Contains("Functions") || declaring.IsSubclassOf(typeof(Delegate)))));

        var converterParameters = parameters.Where(x => x.Member is MethodInfo && typeof(IConverter).IsAssignableFrom(x.Member.DeclaringType)).ToList();
        var converterExpectedParameters = converterParameters.Where(x => !x.Member.Name.StartsWith("Throw") && !x.Member.Name.StartsWith("Except") && Equals(x.ParameterType.Name, names)).ToList();
        var ignoredParameters = converterExpectedParameters.Where(x => !x.ParameterType.IsByRef).ToList();
        var knownIssues = new[] { "IPAddressConverter", "IPEndPointConverter", "VersionConverter", "BigIntegerConverter", "BitArrayConverter", "VariableBoundArrayConverter`2" };
        var parametersWithIssue = ignoredParameters.Where(x => knownIssues.Contains(x.Member.ReflectedType?.Name)).ToList();
        var parametersWithoutIssue = ignoredParameters.Except(parametersWithIssue).ToList();
        Assert.Empty(parametersWithoutIssue);
    }

    [Fact(DisplayName = "Class Should Be Abstract Or Sealed")]
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

    [Fact(DisplayName = "Public Type With Limited Namespace")]
    public void PublicTypes()
    {
        var array = new[]
        {
            "Mikodev.Binary",
            "Mikodev.Binary.Attributes",
            "Mikodev.Binary.Components",
        };
        var types = new HashSet<Type>(typeof(IConverter).Assembly.GetTypes());
        var alpha = new HashSet<Type>(types.Where(x => array.Contains(x.Namespace) && !x.IsNested));
        var bravo = new HashSet<Type>(types.Where(x => x.Name.Any(c => c is '<' or '>' or '-')));
        var delta = new HashSet<Type>(types.Except(alpha).Except(bravo));
        Assert.Equal(types.Count, alpha.Count + bravo.Count + delta.Count);
        Assert.Equal(types, [.. alpha.Union(bravo).Union(delta)]);
        Assert.True(alpha.All(x => x.IsPublic));
        Assert.True(delta.All(x => !x.IsPublic));
    }

    [Fact(DisplayName = "Public Class Object Method Should Be Invisible")]
    public void PublicObjectMethods()
    {
        var types = typeof(IConverter).Assembly.GetTypes()
            .Where(x => (x.IsPublic || x.IsNestedPublic) && !(x.IsAbstract && x.IsSealed) && !typeof(Delegate).IsAssignableFrom(x) && !x.IsInterface && x.Namespace is not "Mikodev.Binary.Attributes")
            .ToList();
        Assert.Equal(5, types.Count);
        foreach (var t in types)
        {
            var equalMethod = t.GetMethodNotNull("Equals", [typeof(object)]);
            var hashMethod = t.GetMethodNotNull("GetHashCode", Type.EmptyTypes);
            var stringMethod = t.GetMethodNotNull("ToString", Type.EmptyTypes);
            var attributes = new[] { equalMethod, hashMethod, stringMethod }.Select(x => x.GetCustomAttribute<EditorBrowsableAttribute>()).ToList();
            Assert.Equal(3, attributes.Count);
            Assert.All(attributes, Assert.NotNull);
            Assert.True(attributes.All(x => Assert.IsType<EditorBrowsableAttribute>(x, exactMatch: false).State == EditorBrowsableState.Never));
        }
    }

    [Fact(DisplayName = "Public ByRef Struct Obsolete 'Equals' And 'GetHashCode'")]
    public void PublicByReferenceStructuresObsoleteObjectMethodsTest()
    {
        static void AssertMessage(MethodInfo method)
        {
            var attribute = method.GetCustomAttributes().OfType<ObsoleteAttribute>().Single();
            Assert.False(attribute.IsError);
            var reflected = method.ReflectedType;
            Assert.NotNull(reflected);
            var message = $"{method.Name}() on {reflected.Name} will always throw an exception.";
            Assert.Equal(message, attribute.Message);
        }

        var types = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsPublic && x.IsByRefLike)
            .ToList();
        Assert.Equal(2, types.Count);
        foreach (var t in types)
        {
            var methods = t.GetMethods().Where(x => x.Name is "Equals" or "GetHashCode").ToList();
            _ = Assert.Single(methods, x => x.Name is "Equals");
            _ = Assert.Single(methods, x => x.Name is "GetHashCode");
            Assert.All(methods, AssertMessage);
        }
    }

    [Fact(DisplayName = "Struct Instance Member Should Be Read Only")]
    public void StructMethods()
    {
        static bool HasReadOnlyAttribute(MemberInfo info) => info.GetCustomAttributes().SingleOrDefault(x => x.GetType().Name == "IsReadOnlyAttribute") != null;
        static bool HasCompilerGeneratedAttribute(MemberInfo info) => info.GetCustomAttributes().SingleOrDefault(x => x.GetType().Name == "CompilerGeneratedAttribute") != null;

        var types = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsValueType && HasCompilerGeneratedAttribute(x) is false).ToList();
        var readonlyTypes = types.Where(HasReadOnlyAttribute).ToList();
        var otherTypes = types.Except(readonlyTypes).ToList();
        var methods = otherTypes.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var ignoreMembers = methods.Where(x => x.DeclaringType == typeof(object) || x.DeclaringType == typeof(ValueType)).ToList();
        var remainMembers = methods.Except(ignoreMembers).ToList();
        var attributes = remainMembers.Select(x => (Method: x, Flag: HasReadOnlyAttribute(x))).ToList();

        Assert.NotEmpty(attributes);
        Assert.Equal(otherTypes.Count * 6 - 3, ignoreMembers.Count);
        Assert.All(attributes, x => Assert.True(x.Flag));
    }

    [Fact(DisplayName = "Debugger Display")]
    public void DebuggerDisplay()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var selection = types.Where(x => x.Name.Contains('<') is false);
        var attributes = selection.Select(x => (Type: x, Attribute: x.GetCustomAttribute<DebuggerDisplayAttribute>(inherit: false))).Where(x => x.Attribute is not null).ToList();
        var overridden = types.Where(x => x.Name.Contains('<') is false && x.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Any(x => x.Name is "EqualityContract") is false && x.GetMethod("ToString")?.DeclaringType == x).ToList();
        Assert.Equal(6, overridden.Count);
        Assert.Equal([.. overridden], attributes.Select(x => x.Type).ToHashSet());
        var display = attributes.Select(x => x.Attribute?.Value).Distinct().Single();
        Assert.Equal("{ToString(),nq}", display);
    }

    [Fact(DisplayName = "Public Method With Byte Array Parameter")]
    public void ByteArray()
    {
        var types = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsPublic).ToList();
        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)).ToList();
        var methodBases = members.OfType<MethodBase>().ToList();
        Assert.NotEmpty(methodBases);
        Assert.NotEmpty(methodBases.OfType<ConstructorInfo>());

        var parameters = methodBases.SelectMany(x => x.GetParameters()).ToList();
        var byteArrayParameters = parameters.Where(x => x.ParameterType == typeof(byte[])).ToList();
        Assert.Equal(9, byteArrayParameters.Count);
        var context = new NullabilityInfoContext();
        foreach (var x in byteArrayParameters)
        {
            var info = context.Create(x);
            Assert.StartsWith("Decode", x.Member.Name);
            Assert.Equal(NullabilityState.Nullable, info.ReadState);
            Assert.Equal(NullabilityState.Nullable, info.WriteState);
        }
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
        Assert.All(attributedMethods, x => Assert.True((x.DeclaringType?.Name is "ThrowHelper" && x.Name.StartsWith("Throw")) || x.Name.StartsWith("Except")));

        var expectedMethods = methods.Where(x => x.Name.Contains("Throw") || x.Name.Contains("Except")).ToList();
        Assert.Equal([.. attributedMethods], [.. expectedMethods]);

        var misspelledMethods = methods.Where(x => x.Name.Contains("Expect", StringComparison.InvariantCultureIgnoreCase)).ToList();
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
        var array = types.Where(x => x.IsPublic is false && x.IsSubclassOf(typeof(Delegate)) is false && x.IsEnum is false && x.FullName?.Contains('<') is false && x.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic).Any(x => x.Name is "EqualityContract") is false).ToList();
        var filter = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Assert.NotEmpty(array);
        foreach (var t in array)
        {
            var fields = t.GetFields(filter);
            var methods = t.GetMethods(filter);
            var constructors = t.GetConstructors(filter);
            var properties = t.GetProperties(filter);
            Assert.All(properties, x => Assert.True(Assert.IsType<MethodInfo>(x.GetGetMethod(), exactMatch: false).IsPublic));
            Assert.All(fields, x => Assert.True(x.IsPublic || x.IsPrivate));
            Assert.All(methods, x => Assert.True(x.Name.Contains('<') || x.DeclaringType == typeof(object) || x.IsPublic || x.IsPrivate));
            Assert.All(constructors, x => Assert.True((Assert.IsType<Type>(x.DeclaringType, exactMatch: false).IsAbstract && x.GetParameters().Length is 0) || x.IsPublic || x.IsPrivate));
        }
    }

    [Fact(DisplayName = "Public Obsolete Members")]
    public void PublicObsoleteMembers()
    {
        var publicMembers = typeof(Allocator).GetMembers().Where(x => x.Name is "GetHashCode" or "Equals");
        var obsoleteAttributes = publicMembers.SelectMany(x => x.GetCustomAttributes(true)).OfType<ObsoleteAttribute>().ToList();
        Assert.Equal(2, obsoleteAttributes.Count);
        Assert.All(obsoleteAttributes, x => Assert.EndsWith("Allocator will always throw an exception.", x.Message));
    }

    [Fact(DisplayName = "Public Method Parameter Nullability")]
    public void PublicEncodeMethodNullability()
    {
        static bool Filter(ParameterInfo p) => p.ParameterType.IsGenericParameter || p.ParameterType == typeof(object) || p.ParameterType == typeof(byte[]);

        var context = new NullabilityInfoContext();
        var publicTypes = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsPublic).ToImmutableArray();
        var publicMethods = publicTypes.SelectMany(x => x.GetMethods()).ToImmutableArray();
        var parameters = publicMethods.SelectMany(x => x.GetParameters()).Where(Filter).ToImmutableArray();
        var selections = parameters.Select(x => KeyValuePair.Create(x, context.Create(x))).ToList();
        Assert.All(selections, x => Assert.Equal(x.Value.ReadState, x.Value.WriteState));
        var groups = selections.GroupBy(x => x.Value.ReadState).ToDictionary(x => x.Key);
        var anonymous = groups[NullabilityState.Nullable].Select(x => x.Key).Where(x => x.Name is "anonymous").ToList();
        Assert.Equal(2, groups.Count);
        Assert.Equal(7, anonymous.Count);
        Assert.True(groups.ContainsKey(NullabilityState.Unknown));
        Assert.True(groups.ContainsKey(NullabilityState.Nullable));
        Assert.All(groups[NullabilityState.Unknown], x => Assert.True(x.Key.Member.DeclaringType?.IsSubclassOf(typeof(Delegate))));
    }

    [Fact(DisplayName = "Public Method With Non-ByRef Ref-Like Type Parameter Should Be Scoped")]
    public void PublicMethodWithNonByRefRefLikeTypeShouldBeScoped()
    {
        var publicTypes = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsPublic).ToList();
        var publicMethods = publicTypes.SelectMany(x => x.GetMethods()).ToList();
        var parameters = publicMethods.SelectMany(x => x.GetParameters()).ToList();
        var matches = parameters.Where(x => x.ParameterType.IsByRef is false && x.ParameterType.IsByRefLike).ToList();
        Assert.NotEmpty(matches);
        var hostTypes = matches.Select(x => x.Member.ReflectedType).Distinct().ToList();
        Assert.Equal(6, hostTypes.Count);
        foreach (var parameter in matches)
        {
            var attributes = parameter.GetCustomAttributes();
            var attribute = Assert.Single(attributes, x => x.GetType().Name is "ScopedRefAttribute");
            Assert.Equal("System.Runtime.CompilerServices", attribute.GetType().Namespace);
        }
    }

    [Fact(DisplayName = "Public Abstract Class Constructor Should Be Protected")]
    public void PublicAbstractClassConstructorShouldBeProtected()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var publicAbstractTypes = types.Where(x => x.IsPublic && x.IsAbstract && x.IsInterface is false && x.IsSealed is false).ToList();
        var knownTypeNames = new HashSet<string> { "Converter`1", "NamedObjectConverter`1" };
        Assert.Equal(knownTypeNames, [.. publicAbstractTypes.Select(x => x.Name)]);

        foreach (var type in publicAbstractTypes)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.NotEmpty(constructors);
            Assert.All(constructors, x => Assert.True(x.IsFamily));
            Assert.All(constructors, x => Assert.False(x.IsPublic));
            Assert.All(constructors, x => Assert.False(x.IsPrivate));
        }
    }
}
