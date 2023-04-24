namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.AbstractTypes;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public interface IPerson
{
    int Age { get; }

    string Name { get; }
}

public abstract class AbstractPerson : IPerson
{
    public virtual int Age { get; }

    public virtual string Name { get; }

    public AbstractPerson(int age, string name)
    {
        Age = age;
        Name = name;
    }
}

public class Person : AbstractPerson, IEquatable<Person?>
{
    public override int Age => base.Age;

    public override string Name => base.Name;

    public Person(int age, string name) : base(age, name) { }

    public bool Equals(Person? other) => other is not null && Age == other.Age && Name == other.Name;

    public override bool Equals(object? obj) => Equals(obj as Person);

    public override int GetHashCode() => HashCode.Combine(Age, Name);
}

public class PersonWithInterfaceConstructor : Person, IEquatable<PersonWithInterfaceConstructor?>
{
    public override int Age => base.Age;

    public override string Name => base.Name;

    internal PersonWithInterfaceConstructor(int age, string name) : base(age, name) { }

    public bool Equals(PersonWithInterfaceConstructor? other) => other is not null && Age == other.Age && Name == other.Name;

    public override bool Equals(object? obj) => Equals(obj as PersonWithInterfaceConstructor);

    public override int GetHashCode() => HashCode.Combine(Age, Name);
}

[SourceGeneratorContext]
[SourceGeneratorInclude<IPerson>]
[SourceGeneratorInclude<AbstractPerson>]
[SourceGeneratorInclude<Person>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    [Fact(DisplayName = "Abstract Type Test")]
    public void AbstractTypeTest()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();

        var source = new Person(20, "Name");
        var keys = new HashSet<string> { "Age", "Name" };
        var converterInterface = generator.GetConverter<IPerson>();
        var converterInterfaceSecond = generatorSecond.GetConverter<IPerson>();
        var bufferInterface = converterInterface.Encode(source);
        var bufferInterfaceSecond = converterInterfaceSecond.Encode(source);
        Assert.Equal(bufferInterface, bufferInterfaceSecond);
        var tokenInterface = new Token(generator, bufferInterface);
        var tokenInterfaceSecond = new Token(generatorSecond, bufferInterfaceSecond);
        Assert.Equal(keys, tokenInterface.Children.Keys.ToHashSet());
        Assert.Equal(keys, tokenInterfaceSecond.Children.Keys.ToHashSet());

        var converterAbstract = generator.GetConverter<AbstractPerson>();
        var converterAbstractSecond = generatorSecond.GetConverter<AbstractPerson>();
        var bufferAbstract = converterAbstract.Encode(source);
        var bufferAbstractSecond = converterAbstractSecond.Encode(source);
        Assert.Equal(bufferAbstract, bufferAbstractSecond);
        var tokenAbstract = new Token(generator, bufferAbstract);
        var tokenAbstractSecond = new Token(generatorSecond, bufferAbstractSecond);
        Assert.Equal(keys, tokenAbstract.Children.Keys.ToHashSet());
        Assert.Equal(keys, tokenAbstractSecond.Children.Keys.ToHashSet());
    }

    public static IEnumerable<object[]> InstanceData()
    {
        yield return new object[] { new Person(18, "Tom") };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(InstanceData))]
    public void EncodeDecodeTest<T>(T source)
    {
        var generator = Generator.CreateDefaultBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var result = converter.Decode(bufferSecond);
        var resultSecond = converterSecond.Decode(buffer);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);
    }

    public static IEnumerable<object[]> InstanceEncodeOnlyData()
    {
        yield return new object[] { new PersonWithInterfaceConstructor(16, "Anonymous") };
    }

    [Theory(DisplayName = "Encode Only Test")]
    [MemberData(nameof(InstanceEncodeOnlyData))]
    public void EncodeOnlyTest<T>(T source)
    {
        var generator = Generator.CreateDefaultBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(bufferSecond));
        var errorSecond = Assert.Throws<NotSupportedException>(() => converterSecond.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorSecond.Message);
    }

    public static IEnumerable<object[]> InterfaceOrAbstractClassData()
    {
        yield return new object[] { typeof(IPerson), new Person(20, "Alpha") };
        yield return new object[] { typeof(AbstractPerson), new Person(22, "Bravo") };
    }

    [Theory(DisplayName = "Interface Or Abstract Class Encode Only Test")]
    [MemberData(nameof(InterfaceOrAbstractClassData))]
    public void InterfaceOrAbstractClassEncodeOnlyTest<T>(Type wantedType, T source)
    {
        Assert.True(wantedType.IsInterface || wantedType.IsAbstract);
        var method = new Action<object>(EncodeOnlyTest).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { wantedType });
        var result = target.Invoke(this, new object?[] { source });
        Assert.Null(result);
    }
}
