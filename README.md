# Binary

[![GitHub repo size](https://img.shields.io/github/repo-size/afxres/binary)](#)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/afxres/binary/build.yml?branch=main)](#)
[![Coverage Status](https://coveralls.io/repos/github/afxres/binary/badge.svg?branch=main)](https://coveralls.io/github/afxres/binary?branch=main)

| Package                       | Version             | Downloads             | Description   |
| :---------------------------- | :------------------ | :-------------------- | :------------ |
| [`Mikodev.Binary`][PC]        | [![version][VC]](#) | [![downloads][IC]](#) | Main package  |
| [`Mikodev.Binary.FSharp`][PF] | [![version][VF]](#) | [![downloads][IF]](#) | F# converters |

## Overview

Serialization library for C# and F#. It uses the same binary layout as the
[C++ version](https://github.com/afxres/binary-cxx): the same length prefix and the same named object
structure, so data written by one can be read by the other when member names and types match.

Two APIs work on that layout:

- **JIT** — `Generator.CreateDefault()` builds converters at run time with reflection and expression trees.
- **AOT** — a source generator emits converters at compile time; `Generator.CreateAotBuilder()` consumes them.

## Getting Started

```bash
dotnet add package Mikodev.Binary
dotnet add package Mikodev.Binary.FSharp  # F# converters
```

The packages target .NET 10 and .NET 11.

### JIT

```csharp
using Mikodev.Binary;

var generator = Generator.CreateDefault();
var source = new Person("C#", 21);
var buffer = generator.Encode(source);
var result = generator.Decode<Person>(buffer);
Console.WriteLine(result);

record Person(string Name, int Age);
```

See [Wire Format](#wire-format) for the byte layout.

### AOT

Annotate the types the context needs. The context itself must be inside a namespace.

```csharp
namespace SomeNamespace;

using Mikodev.Binary.Attributes;

public record Person(int Id, string Name);

[SourceGeneratorContext]
[SourceGeneratorInclude<Person>]
public partial class SomeSourceGeneratorContext { }
```

The generator adds a `ConverterCreators` property to the context; pass it to the builder:

```csharp
using Mikodev.Binary;
using SomeNamespace;

var generator = Generator.CreateAotBuilder()
    .AddConverterCreators(SomeSourceGeneratorContext.ConverterCreators.Values)
    .Build();
var converter = generator.GetConverter<Person>();
var person = new Person(Id: 1, Name: "Someone");
var buffer = converter.Encode(person);
var result = converter.Decode(buffer);
Console.WriteLine(result);
```

## JIT vs AOT

|                      | JIT                                     | AOT                                                |
| :------------------- | :-------------------------------------- | :------------------------------------------------- |
| Converter discovery  | run time (reflection, expression trees) | compile time (source generator)                    |
| Setup                | `Generator.CreateDefault()`             | `CreateAotBuilder()` + `AddConverterCreators(...)` |
| Trimming / NativeAOT | requires dynamic code                   | supported                                          |
| Anonymous types      | yes                                     | no                                                 |

Feature support:

| Feature                   | JIT | AOT |
| :------------------------ | :-- | :-- |
| Enumeration Types         | Yes | Yes |
| Anonymous Types           | Yes |     |
| Tuple Types               | Yes | Yes |
| Records                   | Yes | Yes |
| Required Members          | Yes | Yes |
| Inline Arrays             | Yes | Yes |
| Union                     | Yes | Yes |
| Discriminated Unions (F#) | Yes |     |

## Supported Types

| Category           | Types                                                                                                                                                   |
| :----------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Primitive          | `Boolean`, `Byte`, `SByte`, `Char`, `Int16`, `Int32`, `Int64`, `Int128`, `UInt16`, `UInt32`, `UInt64`, `UInt128`, `Half`, `Single`, `Double`, `Decimal` |
| String             | `String`, encoded as UTF-8                                                                                                                              |
| Date & time        | `DateOnly`, `DateTime`, `DateTimeOffset`, `TimeOnly`, `TimeSpan`                                                                                        |
| Numeric            | `BigInteger`, `Complex`, `Index`, `Range`, `BitVector32`, `Matrix3x2`, `Matrix4x4`, `Plane`, `Quaternion`, `Vector2`, `Vector3`, `Vector4`              |
| Memory & sequences | `T[]`, `Memory<>`, `ReadOnlyMemory<>`, `ArraySegment<>`, `ReadOnlySequence<>`, `ImmutableArray<>`                                                       |
| Tuple              | `ValueTuple<...>`, `Tuple<...>`, `KeyValuePair<,>`                                                                                                      |
| Collection         | `IEnumerable<>` with a constructor that accepts `IEnumerable<>`, plus `List<>`, `HashSet<>`, `Dictionary<,>` and the immutable/frozen collections       |
| Miscellaneous      | `BitArray`, `Guid`, `IPAddress`, `IPEndPoint`, `Nullable<>`, `PriorityQueue<,>`, `Rune`, `Uri`, `Version`, `LinkedList<>`                               |

A few details:

- Enumeration types are encoded as their underlying integer.
- `Stack<>` and the other stack types are explicitly rejected.
- `Tuple<...>` cannot be null.
- .NET 11 additionally supports `BFloat16`, `Decimal32`, `Decimal64` and `Decimal128`.

## Wire Format

### Length Prefix

| Value                | Leading Bit | Bytes                       | Example              |
| :------------------- | :---------- | :-------------------------- | :------------------- |
| `0 ~ 0x7F`           | `0`         | 1                           | `7F` → 127           |
| `0x80 ~ 0x7FFF_FFFF` | `1`         | 4, big-endian, high bit set | `80 00 04 01` → 1025 |

### Named Object

`new { id = 1024, name = "C#" }` (16 bytes):

| Bytes            | Meaning                                       |
| :--------------- | :-------------------------------------------- |
| `02 69 64`       | key `"id"`, length 2                          |
| `04 00 04 00 00` | value `1024`, length 4, little-endian `int32` |
| `04 6e 61 6d 65` | key `"name"`, length 4                        |
| `02 43 23`       | value `"C#"`, length 2                        |

Both the key and the value carry a length prefix. Members are written in name order to keep the
output deterministic (the tests rely on it), but the format does not require any order: decoding
looks members up by name.

### Tuple

```csharp
var time = (Seconds: 1712L, Nanos: 500);
var user = (Name: "amy", Id: 7);
var value = (Time: time, User: user, Action: "login");
```

| Bytes                     | Meaning                                  |
| :------------------------ | :--------------------------------------- |
| `b0 06 00 00 00 00 00 00` | `Time.Seconds = 1712`                    |
| `f4 01 00 00`             | `Time.Nanos = 500`                       |
| `03 61 6d 79`             | `User.Name = "amy"`, length 3            |
| `07 00 00 00`             | `User.Id = 7`                            |
| `6c 6f 67 69 6e`          | `Action`, last element, no length prefix |

Elements are concatenated in order: a variable length element is prefixed unless it is the last
element, whose length is implied by the remaining bytes. Nested tuples are flattened, so
`((1712L, 500), ("amy", 7), "login")` and `(1712L, 500, "amy", 7, "login")` produce the same 25 bytes.

### Array

`new int[] { 1, 2 }` (8 bytes) — elements only, no count:

```
01 00 00 00 02 00 00 00
```

`new string[] { "C#", ".Net" }` (8 bytes):

| Bytes            | Meaning            |
| :--------------- | :----------------- |
| `02 43 23`       | `"C#"`, length 2   |
| `04 2e 4e 65 74` | `".Net"`, length 4 |

## License

[MIT](LICENSE)

[PC]:https://www.nuget.org/packages/Mikodev.Binary/
[PF]:https://www.nuget.org/packages/Mikodev.Binary.FSharp/
[VC]:https://img.shields.io/nuget/vpre/Mikodev.Binary
[VF]:https://img.shields.io/nuget/vpre/Mikodev.Binary.FSharp
[IC]:https://img.shields.io/nuget/dt/Mikodev.Binary
[IF]:https://img.shields.io/nuget/dt/Mikodev.Binary.FSharp
