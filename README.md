# Binary

[![GitHub repo size](https://img.shields.io/github/repo-size/afxres/binary)](#)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/afxres/binary/build.yml?branch=main)](#)
[![Coverage Status](https://coveralls.io/repos/github/afxres/binary/badge.svg?branch=main)](https://coveralls.io/github/afxres/binary?branch=main)

| Package                       | Version             | Downloads             | Descriptions                 |
| :---------------------------- | :------------------ | :-------------------- | :--------------------------- |
| [`Mikodev.Binary`][PC]        | [![version][VC]](#) | [![downloads][IC]](#) | Main package                 |
| [`Mikodev.Binary.FSharp`][PF] | [![version][VF]](#) | [![downloads][IF]](#) | Additional converters for F# |

## Getting Started

### Sample code

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

### Supported types

| Category      | Details                                                                                                                                      | Comment                                  |
| :------------ | :------------------------------------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
| Primitive     | ``(U)Int(16,32,64,128)``, ``Boolean``, ``Byte``, ``Char``, ``Decimal``, ``Double``, ``Half``, ``SByte``, ``Single``, ``String``              | Default string encoding is UTF-8         |
| Data & Time   | ``DateOnly``, ``DateTime``, ``DateTimeOffset``, ``TimeOnly``, ``TimeSpan``                                                                   |                                          |
| Numeric       | ``BigInteger``, ``Complex``, ``Matrix3x2``, ``Matrix4x4``, ``Plane``, ``Quaternion``, ``Vector2``, ``Vector3``, ``Vector4``                  |                                          |
| Memory        | ``T[...]``, ``Memory<>``, ``ReadOnlyMemory<>``, ``ReadOnlySequence<>``                                                                       |                                          |
| Tuple         | ``KeyValuePair<,>``, ``Tuple<...>``, ``ValueTuple<...>``                                                                                     | Tuple can not be null                    |
| Miscellaneous | ``BitArray``, ``BitVector32``, ``Guid``, ``IPAddress``, ``IPEndPoint``, ``Nullable<>``, ``PriorityQueue<,>``, ``Rune``, ``Uri``, ``Version`` |                                          |
| Collection    | Implements ``IEnumerable<>`` and have a constructor accept ``IEnumerable<>`` as parameter                                                    | Stack types are explicitly not supported |

### Language Features

| Feature                   | JIT  | AOT  | Comment            |
| :------------------------ | :--- | :--- | :----------------- |
| Enumeration Types         | Yes  | Yes  | Handle as integers |
| Anonymous Types           | Yes  |      |                    |
| Tuple Types               | Yes  | Yes  |                    |
| Records                   | Yes  | Yes  |                    |
| Required Members          | Yes  | Yes  |                    |
| Inline Arrays             | Yes  | Yes  |                    |
| Discriminated Unions (F#) | Yes  |      |                    |

### AOT Support

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

## Binary Layout

### Length Prefix

| Leading Bit | Byte Length | Range               | Example Bytes   | Example Value |
| :---------- | :---------- | :------------------ | :-------------- | :------------ |
| ``0``       | ``1``       | ``0 ~ 0x7F``        | ``7F``          | ``127``       |
| ``1``       | ``4``       | ``0 ~ 0x7FFF_FFFF`` | ``80 00 04 01`` | ``1025``      |

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
