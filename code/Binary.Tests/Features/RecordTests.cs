namespace Mikodev.Binary.Tests.Features;

using System.Collections.Generic;
using Xunit;

public class RecordTests
{
    record Person(int Id, string Name);

    record struct ValuePerson(int Id, string Name);

    public static readonly IEnumerable<object[]> DataRecordArguments = new[]
    {
        new object[] { new Person(1024, "Sharp"), new ValuePerson(1024, "Sharp") },
    };

    [Theory(DisplayName = "Record And Record Struct Cross Test")]
    [MemberData(nameof(DataRecordArguments))]
    public static void RecordAndRecordStructCrossTest<T, R>(T a, R b)
    {
        var generator = Generator.CreateDefault();
        var h = generator.Encode(a);
        var i = generator.Encode(b);
        Assert.Equal<byte>(h, i);
        var j = generator.Decode<T>(h);
        var k = generator.Decode<R>(i);
        Assert.Equal(a, j);
        Assert.Equal(b, k);
    }
}
