namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Xunit;

public class TrimAnalyzerTests
{
    private static bool MemberFilter(MemberInfo member) => member switch
    {
        ConstructorInfo constructor => constructor.ReflectedType?.IsPublic is true && constructor.IsPublic,
        MethodInfo method => method.ReflectedType?.IsPublic is true && method.IsPublic,
        _ => throw new NotSupportedException(),
    };

    [Fact(DisplayName = "Known 'RequiresUnreferencedCodeAttribute' Members")]
    public void KnownRequiresUnreferencedCodeMembers()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var classAttributes = types.ToDictionary(x => x, x => x.GetCustomAttributes(true).OfType<RequiresUnreferencedCodeAttribute>().SingleOrDefault());
        var classAttributeTypes = classAttributes.Where(x => x.Value is not null).ToDictionary(x => x.Key, x => x.Value);
        Assert.All(classAttributeTypes, x => Assert.False(x.Key.IsPublic));

        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var memberAttributes = members.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var memberAttributeTypes = memberAttributes.Values.SelectMany(x => x).Select(x => x.GetType()).Distinct().ToList();
        Assert.Contains(typeof(RequiresUnreferencedCodeAttribute), memberAttributeTypes);

        var memberWithKnownAttributes = memberAttributes.Where(x => x.Value.Any(x => x is RequiresUnreferencedCodeAttribute)).ToImmutableDictionary();
        var publicTypeMemberWithKnownAttributes = memberWithKnownAttributes.Where(x => MemberFilter(x.Key)).ToImmutableDictionary();
        Assert.NotEmpty(publicTypeMemberWithKnownAttributes);
        Assert.True(publicTypeMemberWithKnownAttributes.Count < memberWithKnownAttributes.Count);
        Assert.All(publicTypeMemberWithKnownAttributes, x => Assert.StartsWith("CreateDefault", x.Key.Name));
    }

    [Fact(DisplayName = "Known 'RequiresDynamicCodeAttribute' Message")]
    public void KnownRequiresDynamicCodeMessage()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var memberAttributes = members.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var memberWithKnownAttributes = memberAttributes.Where(x => x.Value.Any(x => x is RequiresDynamicCodeAttribute)).ToImmutableDictionary();
        var messages = memberWithKnownAttributes.Select(x => x.Value.OfType<RequiresDynamicCodeAttribute>().Single().Message).ToHashSet();
        var message = Assert.Single(messages);
        Assert.Equal("Requires dynamic code for binary serialization.", message);
    }

    [Fact(DisplayName = "Known 'RequiresUnreferencedCodeAttribute' Message")]
    public void KnownRequiresUnreferencedCodeMessage()
    {
        var types = typeof(IConverter).Assembly.GetTypes();
        var members = types.SelectMany(x => x.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)).ToList();
        var memberAttributes = members.ToImmutableDictionary(x => x, x => x.GetCustomAttributes(true).Cast<Attribute>().ToList());
        var memberWithKnownAttributes = memberAttributes.Where(x => x.Value.Any(x => x is RequiresUnreferencedCodeAttribute)).ToImmutableDictionary();
        var messages = memberWithKnownAttributes.Select(x => x.Value.OfType<RequiresUnreferencedCodeAttribute>().Single().Message).ToHashSet();
        var message = Assert.Single(messages);
        Assert.Equal("Requires public members for binary serialization.", message);
    }
}
