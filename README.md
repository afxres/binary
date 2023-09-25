# Binary

Summary:

![GitHub repo size](https://img.shields.io/github/repo-size/afxres/binary)
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/afxres/binary/dotnet-tests.yml?branch=main)
[![Coverage Status](https://coveralls.io/repos/github/afxres/binary/badge.svg?branch=main)](https://coveralls.io/github/afxres/binary?branch=main)

| Package                       | Version        | Downloads        | Descriptions        |
| :---------------------------- | :------------- | :--------------- | :------------------ |
| [`Mikodev.Binary`][PC]        | ![version][VC] | ![downloads][IC] | Main package        |
| [`Mikodev.Binary.FSharp`][PF] | ![version][VF] | ![downloads][IF] | Type support for F# |

Add package (for C# project):
```
dotnet add package Mikodev.Binary
```

Add package (for F# project):
```
dotnet add package Mikodev.Binary.FSharp
```

Sample code:
```csharp
using Mikodev.Binary;
using System;

var generator = Generator.CreateDefault();
var source = new Person("C#", 21);
var buffer = generator.Encode(source);
var result = generator.Decode<Person>(buffer);
Console.WriteLine(result);

record Person(string Name, int Age);
```

Supported types:
| Category      | Details                                                                                                                         | Comment                                  |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| Primitive     | ``(U)Int(16,32,64,128)``, ``Boolean``, ``Byte``, ``Char``, ``Decimal``, ``Double``, ``Half``, ``SByte``, ``Single``, ``String`` | Default encoding of string is 'UTF-8'    |
| Data & Time   | ``DateOnly``, ``DateTime``, ``DateTimeOffset``, ``TimeOnly``, ``TimeSpan``                                                      |                                          |
| Numeric       | ``BigInteger``, ``Complex``, ``Matrix3x2``, ``Matrix4x4``, ``Plane``, ``Quaternion``, ``Vector2``, ``Vector3``, ``Vector4``     |                                          |
| Memory        | ``T[...]``, ``Memory<>``, ``ReadOnlyMemory<>``, ``ReadOnlySequence<>``                                                          |                                          |
| Miscellaneous | ``BitVector32``, ``Guid``, ``IPAddress``, ``IPEndPoint``, ``Nullable<>``, ``PriorityQueue<,>``, ``Rune``, ``Uri``, ``Version``  |                                          |
| Tuple         | ``KeyValuePair<,>``, ``Tuple<...>``, ``ValueTuple<...>``                                                                        | Tuple can not be null                    |
| Collection    | Implements ``IEnumerable<>`` and have a constructor accept ``IEnumerable<>`` as parameter                                       | Stack types are explicitly not supported |

Language Features:
| Feature                   | JIT | AOT | Comment                              |
| ------------------------- | --- | --- | ------------------------------------ |
| Enumeration Types         | Yes | Yes | Use underlying integral numeric type |
| Anonymous Types           | Yes |     |                                      |
| Tuple Types               | Yes | Yes |                                      |
| Records                   | Yes | Yes |                                      |
| Required Members          | Yes | Yes |                                      |
| Inline Arrays             | Yes | Yes |                                      |
| Discriminated Unions (F#) | Yes |     |                                      |

## AOT Support

AOT support (via source generator) is now generally available.  
For example, we have a data model like this:
```csharp
record Person(int Id, string Name);
```

Then create a partial type with ``SourceGeneratorContextAttribute`` and include this data model:
```csharp
namespace SomeNamespace;

using Mikodev.Binary.Attributes;

[SourceGeneratorContext]
[SourceGeneratorInclude<Person>]
partial class SomeSourceGeneratorContext { }
```

This will generate a property named ``ConverterCreators`` which contains all generated converter creators.  
Just add those converter creators to ``IGenerator`` and it will work.
```csharp
var generator = Generator.CreateAotBuilder()
    .AddConverterCreators(SomeSourceGeneratorContext.ConverterCreators.Values)
    .Build();
var converter = generator.GetConverter<Person>();
var person = new Person(Id: 1, Name: "Someone");
var buffer = converter.Encode(person);
var result = converter.Decode(buffer);
Console.WriteLine(result.Id);   // 1
Console.WriteLine(result.Name); // Someone
```

## Implement custom converters

Data model:
```csharp
record Person(string Name, int Age);
```

A simple converter implementation:
```csharp
class SimplePersonConverter : Converter<Person>
{
    public override void Encode(ref Allocator allocator, Person item)
    {
        Allocator.Append(ref allocator, sizeof(int), item.Age, BinaryPrimitives.WriteInt32LittleEndian);
        Allocator.Append(ref allocator, item.Name.AsSpan(), Encoding.UTF8);
    }

    public override Person Decode(in ReadOnlySpan<byte> span)
    {
        var age = BinaryPrimitives.ReadInt32LittleEndian(span);
        var name = Encoding.UTF8.GetString(span.Slice(sizeof(int)));
        return new Person(name, age);
    }
}
```

Or implement with existing converters via converter creator:
```csharp
class SimplePersonConverter(Converter<int> intConverter, Converter<string> stringConverter) : Converter<Person>
{
    public override void Encode(ref Allocator allocator, Person item)
    {
        intConverter.Encode(ref allocator, item.Age);
        stringConverter.Encode(ref allocator, item.Name);
    }

    public override Person Decode(in ReadOnlySpan<byte> span)
    {
        var age = intConverter.Decode(span);
        var name = stringConverter.Decode(span.Slice(sizeof(int)));
        return new Person(name, age);
    }
}

class SimplePersonConverterCreator : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type != typeof(Person))
            return null;
        var intConverter = (Converter<int>)context.GetConverter(typeof(int));
        var stringConverter = (Converter<string>)context.GetConverter(typeof(string));
        return new SimplePersonConverter(intConverter, stringConverter);
    }
}
```

Then add this converter creator to ``IGenerator``:
```csharp
var generator = Generator.CreateDefaultBuilder()
    .AddConverterCreator(new SimplePersonConverterCreator())
    .Build();
var converter = generator.GetConverter<Person>();
var source = new Person("C#", 21);
var buffer = converter.Encode(source);
var result = converter.Decode(buffer);
Console.WriteLine(result);
```

## Binary Layout

### Length Prefix

Variable length codes for length prefix:
| Leading Bit | Byte Length | Range            | Example Bytes   | Example Value |
| ----------- | ----------- | ---------------- | --------------- | ------------- |
| ``0``       | ``1``       | ``0 ~ 2^7 - 1``  | ``7F``          | ``127``       |
| ``1``       | ``4``       | ``0 ~ 2^31 - 1`` | ``80 00 04 01`` | ``1025``      |

### Object

Value:
```csharp
new { id = 1024, name = "C#" }
```

Equivalent to:
```csharp
new SortedDictionary<string, object> { ["id"] = 1024, ["name"] = "C#" }
```

Bytes:
```
    i  d                1024
02  69 64           04  00 04 00 00
    n  a  m  e          C  #
04  6e 61 6d 65     02  43 23
```

### Tuple

Value:
```csharp
("Text", 3.14F)
```

Bytes:
```
    T  e  x  t
04  54 65 78 74
    3.14
    c3 f5 48 40
```

[PC]:https://www.nuget.org/packages/Mikodev.Binary/
[PF]:https://www.nuget.org/packages/Mikodev.Binary.FSharp/
[VC]:https://img.shields.io/nuget/vpre/Mikodev.Binary
[VF]:https://img.shields.io/nuget/vpre/Mikodev.Binary.FSharp
[IC]:https://img.shields.io/nuget/dt/Mikodev.Binary
[IF]:https://img.shields.io/nuget/dt/Mikodev.Binary.FSharp
