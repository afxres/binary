﻿namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Xunit;

public class TrimAnalyzerTests
{
    private static bool MemberFilter(MemberInfo member)
    {
        return member switch
        {
            ConstructorInfo constructor => constructor.ReflectedType?.IsPublic is true && constructor.IsPublic,
            MethodInfo method => method.ReflectedType?.IsPublic is true && method.IsPublic,
            _ => throw new NotSupportedException(),
        };
    }

    [Fact(DisplayName = "Known 'RequiresUnreferencedCodeAttribute' Members")]
    public void KnownRequiresUnreferencedCodeMembers()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var classAttributes = types.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var classAttributeTypes = classAttributes.Values.SelectMany(x => x).Select(x => x.GetType()).Distinct().ToList();
        Assert.DoesNotContain(typeof(RequiresUnreferencedCodeAttribute), classAttributeTypes);

        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var memberAttributes = members.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var memberAttributeTypes = memberAttributes.Values.SelectMany(x => x).Select(x => x.GetType()).Distinct().ToList();
        Assert.Contains(typeof(RequiresUnreferencedCodeAttribute), memberAttributeTypes);

        var memberWithKnownAttributes = memberAttributes.Where(x => x.Value.Any(x => x is RequiresUnreferencedCodeAttribute)).ToImmutableDictionary();
        var publicTypeMemberWithKnownAttributes = memberWithKnownAttributes.Where(x => MemberFilter(x.Key)).ToImmutableDictionary();
        Assert.NotEmpty(publicTypeMemberWithKnownAttributes);
        Assert.True(publicTypeMemberWithKnownAttributes.Count < memberWithKnownAttributes.Count);

        var publicExpectedKnownTypes = new HashSet<Type>()
        {
            typeof(IConverterCreator),
            typeof(IGenerator),
            typeof(IGeneratorContext),
            typeof(Generator),
            typeof(GeneratorExtensions),
            typeof(Token),
        };
        var publicActualKnownTypes = publicTypeMemberWithKnownAttributes.Select(x => x.Key.DeclaringType!).ToHashSet();
        Assert.NotEmpty(publicActualKnownTypes);
        Assert.Equal(publicExpectedKnownTypes, publicActualKnownTypes);
    }

    [Fact(DisplayName = "Known 'RequiresUnreferencedCodeAttribute' Message")]
    public void KnownRequiresUnreferencedCodeMessage()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var memberAttributes = members.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var memberAttributeTypes = memberAttributes.Values.SelectMany(x => x).Select(x => x.GetType()).Distinct().ToList();
        var memberWithKnownAttributes = memberAttributes.Where(x => x.Value.Any(x => x is RequiresUnreferencedCodeAttribute)).ToImmutableDictionary();
        var messages = memberWithKnownAttributes.Select(x => x.Value.OfType<RequiresUnreferencedCodeAttribute>().Single().Message).ToHashSet();
        var message = Assert.Single(messages);
        Assert.Equal("Require public members for binary serialization.", message);
    }
}
