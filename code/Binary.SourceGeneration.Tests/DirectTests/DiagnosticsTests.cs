namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Xunit;

public class DiagnosticsTests
{
    // The identifier set is pinned so that removing, renaming, or renumbering a descriptor is a
    // deliberate edit instead of a silent one. Adding a descriptor means adding it here as well.
    private static ImmutableArray<string> ExpectedDiagnosticIds { get; } =
    [
        "BINSRCGEN01", "BINSRCGEN02", "BINSRCGEN03", "BINSRCGEN04", "BINSRCGEN05",
        "BINSRCGEN11", "BINSRCGEN12", "BINSRCGEN13",
        "BINSRCGEN21", "BINSRCGEN22", "BINSRCGEN23",
        "BINSRCGEN31", "BINSRCGEN32", "BINSRCGEN33", "BINSRCGEN34", "BINSRCGEN35", "BINSRCGEN36",
        "BINSRCGEN41", "BINSRCGEN42", "BINSRCGEN43", "BINSRCGEN44",
        "BINSRCGEN51", "BINSRCGEN52", "BINSRCGEN53", "BINSRCGEN54", "BINSRCGEN55", "BINSRCGEN56", "BINSRCGEN57", "BINSRCGEN58", "BINSRCGEN59", "BINSRCGEN60",
        "BINSRCGEN71", "BINSRCGEN72",
    ];

    private static int[] GetPlaceholderIndexes(string messageFormat)
    {
        return Regex.Matches(messageFormat, @"\{(\d+)\}").Select(x => int.Parse(x.Groups[1].Value)).ToArray();
    }

    [Fact(DisplayName = "Diagnostic Descriptors Test")]
    public void DiagnosticDescriptorsTest()
    {
        var members = typeof(Diagnostics).GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var descriptorProperties = members.OfType<PropertyInfo>().Where(x => x.PropertyType == typeof(DiagnosticDescriptor)).ToList();
        var descriptors = descriptorProperties.Select(x => Assert.IsType<DiagnosticDescriptor>(x.GetValue(null), exactMatch: false)).ToList();
        Assert.NotEmpty(descriptors);

        var ids = descriptors.Select(x => x.Id).OrderBy(x => x, StringComparer.Ordinal).ToList();
        Assert.All(ids, x => Assert.Matches(@"^BINSRCGEN\d\d$", x));
        Assert.Distinct(ids);
        Assert.Equal(ExpectedDiagnosticIds, ids);

        // a single category keeps the IDE grouping and the suppression story simple
        Assert.All(descriptors, x => Assert.Equal(Diagnostics.DiagnosticCategory, x.Category));
        Assert.All(descriptors, x => Assert.True(x.IsEnabledByDefault));

        // placeholders must be contiguous from zero, otherwise the message either leaves a literal
        // brace behind or throws while it is being formatted
        Assert.All(descriptors, x => Assert.Equal(Enumerable.Range(0, GetPlaceholderIndexes(x.MessageFormat.ToString()).Length), GetPlaceholderIndexes(x.MessageFormat.ToString())));
    }
}
