namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Xunit;

public partial class UriConverterTests
{
    public static readonly IEnumerable<object[]> NormalData = new[]
    {
        new object[] { Encoding.UTF8, new Uri("https://github.com") },
        new object[] { Encoding.UTF8, new Uri("https://github.com/dotnet/") },
        new object[] { Encoding.UTF32, new Uri("https://github.com") },
        new object[] { Encoding.UTF32, new Uri("https://github.com/dotnet/") },
    };

    [Theory(DisplayName = "Base Methods")]
    [MemberData(nameof(NormalData))]
    public void Base(Encoding encoding, Uri item)
    {
        var stringConverter = new SimpleStringConverter { Encoding = encoding };
        var generator = Generator.CreateDefaultBuilder().AddConverter(stringConverter).Build();
        var converter = generator.GetConverter<Uri>();
        var a = converter.Encode(item);
        var allocator = new Allocator();
        converter.Encode(ref allocator, item);
        var b = allocator.ToArray();
        var expectedBytes = encoding.GetBytes(item.OriginalString);
        Assert.Equal(expectedBytes, a);
        Assert.Equal(expectedBytes, b);
        Assert.Equal(new[] { "Encode", "Encode" }, stringConverter.CallingSteps);
        stringConverter.CallingSteps.Clear();

        var x = converter.Decode(expectedBytes);
        var span = new ReadOnlySpan<byte>(expectedBytes);
        var y = converter.Decode(in span);
        Assert.Equal(item, x);
        Assert.Equal(item, y);
        Assert.Equal(new[] { "Decode", "Decode" }, stringConverter.CallingSteps);
    }

    [Theory(DisplayName = "Auto Methods")]
    [MemberData(nameof(NormalData))]
    public void Auto(Encoding encoding, Uri item)
    {
        var stringConverter = new SimpleStringConverter { Encoding = encoding };
        var generator = Generator.CreateDefaultBuilder().AddConverter(stringConverter).Build();
        var converter = generator.GetConverter<Uri>();
        var allocator = new Allocator();
        converter.EncodeAuto(ref allocator, item);
        var a = allocator.ToArray();
        var expectedBytes = Allocator.Invoke(item, (ref Allocator allocator, Uri data) => Allocator.AppendWithLengthPrefix(ref allocator, data.OriginalString.AsSpan(), encoding));
        Assert.Equal(expectedBytes, a);
        Assert.Equal(new[] { "EncodeAuto", }, stringConverter.CallingSteps);
        stringConverter.CallingSteps.Clear();

        var span = new ReadOnlySpan<byte>(expectedBytes);
        var x = converter.DecodeAuto(ref span);
        Assert.Equal(item, x);
        Assert.Equal(new[] { "DecodeAuto", }, stringConverter.CallingSteps);
    }

    [Theory(DisplayName = "Length Prefix Methods")]
    [MemberData(nameof(NormalData))]
    public void LengthPrefix(Encoding encoding, Uri item)
    {
        var stringConverter = new SimpleStringConverter { Encoding = encoding };
        var generator = Generator.CreateDefaultBuilder().AddConverter(stringConverter).Build();
        var converter = generator.GetConverter<Uri>();
        var allocator = new Allocator();
        converter.EncodeWithLengthPrefix(ref allocator, item);
        var a = allocator.ToArray();
        var expectedBytes = Allocator.Invoke(item, (ref Allocator allocator, Uri data) => Allocator.AppendWithLengthPrefix(ref allocator, data.OriginalString.AsSpan(), encoding));
        Assert.Equal(expectedBytes, a);
        Assert.Equal(new[] { "EncodeWithLengthPrefix", }, stringConverter.CallingSteps);
        stringConverter.CallingSteps.Clear();

        var span = new ReadOnlySpan<byte>(expectedBytes);
        var x = converter.DecodeWithLengthPrefix(ref span);
        Assert.Equal(item, x);
        Assert.Equal(new[] { "DecodeWithLengthPrefix", }, stringConverter.CallingSteps);
    }

    [Fact(DisplayName = "Null String Converter")]
    public void NullStringConverter()
    {
        var converterType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "UriConverter");
        var parameter = Expression.Parameter(typeof(Converter<string>));
        var constructor = Assert.IsAssignableFrom<ConstructorInfo>(converterType.GetConstructor(new[] { parameter.Type }));
        var lambda = Expression.Lambda<Func<Converter<string>?, Converter<Uri>>>(Expression.New(constructor, parameter), parameter);
        var functor = lambda.Compile();
        var error = Assert.Throws<ArgumentNullException>(() => functor.Invoke(null));
        Assert.Equal(constructor.GetParameters().Single().Name, error.ParamName);
    }

    [Fact(DisplayName = "Null Or Empty")]
    public void NullOrEmpty()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Uri>();
        var a = converter.Encode(null);
        Assert.True(ReferenceEquals(Array.Empty<byte>(), a));
        var x = converter.Decode(a);
        Assert.Null(x);
    }
}
