namespace Mikodev.Binary.WireTests;

using System;
using System.Linq;
using Xunit;

public class WireCoverageTests
{
    [Fact(DisplayName = "Table and corpus have the same names")]
    public void NamesMatch()
    {
        var table = WireFixture.Rows.Select(x => x.Name).Order(StringComparer.Ordinal).ToArray();
        var corpus = WireEncoder.Cases.Keys.Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(corpus, table);
    }

    [Fact(DisplayName = "Duplicate names are rejected")]
    public void NamesAreUnique()
    {
        var duplicates = WireFixture.Rows.GroupBy(x => x.Name, StringComparer.Ordinal)
            .Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        Assert.Empty(duplicates);
    }

    [Fact(DisplayName = "Every corpus type is reachable through the AOT builder")]
    public void EveryTypeHasAnAotConverter()
    {
        var missing = WireEncoder.Cases.Values
            .Where(x => !TryGetConverter(WireEncoder.Aot, x.Type))
            .Select(x => x.Name).Order(StringComparer.Ordinal).ToArray();
        Assert.Empty(missing);
    }

    [Fact(DisplayName = "Every row declares a known group")]
    public void GroupsAreKnown()
    {
        string[] known = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O"];
        var unknown = WireEncoder.Cases.Values.Where(x => !known.Contains(x.Group)).Select(x => $"{x.Name}={x.Group}").ToArray();
        Assert.Empty(unknown);
    }

    private static bool TryGetConverter(Mikodev.Binary.IGenerator generator, Type type)
    {
        try
        {
            _ = generator.GetConverter(type);
            return true;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }
}
