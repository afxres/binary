# Binary

Summary:
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

Sample code (F#):
```fsharp
open Mikodev.Binary

let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
let source = {| text = "Hello, F#!"; list = [ 1; 0; 2; 4 ] |}
let buffer = generator.Encode source
let result = generator.Decode(buffer, anonymous = source)
printfn "%A" result
```

## Implement custom converters

Data model:
```fsharp
type Person = {
    Id : int
    Name : string
}
```

A simple converter implementation:
```fsharp
type SimplePersonConverter() =
    inherit Converter<Person>()

    override __.Encode(allocator : byref<Allocator>, item : Person) : unit =
        Allocator.Append(&allocator, sizeof<int>, item.Id, fun span data -> BinaryPrimitives.WriteInt32LittleEndian(span, data))
        Allocator.Append(&allocator, item.Name.AsSpan(), Encoding.UTF8)
        ()

    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Person =
        let id = BinaryPrimitives.ReadInt32LittleEndian(span)
        let name = Encoding.UTF8.GetString(span.Slice(sizeof<int>))
        { Id = id; Name = name}
```

Or implement with existing converters via converter creator:
```fsharp
type PersonConverter(intConverter : Converter<int>, stringConverter : Converter<string>) =
    inherit Converter<Person>()

    override __.Encode(allocator : byref<Allocator>, item : Person) : unit =
        intConverter.Encode(&allocator, item.Id)
        stringConverter.Encode(&allocator, item.Name)
        ()
    
    override __.Decode(span : inref<ReadOnlySpan<byte>>) : Person =
        let id = intConverter.Decode(&span)
        let name = let part = span.Slice(sizeof<int>) in stringConverter.Decode(&part)
        { Id = id; Name = name}

type PersonConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context : IGeneratorContext, t : Type) =
            if t = typeof<Person> then
                let arguments = [ typeof<int>; typeof<string> ] |> Seq.map context.GetConverter |> Seq.cast<obj> |> Seq.toArray
                let converter = Activator.CreateInstance(typeof<PersonConverter>, arguments)
                converter :?> IConverter
            else
                null
```

And use like this:
```fsharp
let generator =
    Generator.CreateDefaultBuilder()
        .AddConverterCreator(PersonConverterCreator())
        .Build()
let person = { Id = 1024; Name = "F#" }
let buffer = generator.Encode person
printfn "buffer = %A" buffer
let result = generator.Decode<Person> buffer
printfn "result = %A" result
```

[PC]:https://www.nuget.org/packages/Mikodev.Binary/
[PF]:https://www.nuget.org/packages/Mikodev.Binary.FSharp/
[VC]:https://img.shields.io/nuget/v/Mikodev.Binary
[VF]:https://img.shields.io/nuget/v/Mikodev.Binary.FSharp
[IC]:https://img.shields.io/nuget/dt/Mikodev.Binary
[IF]:https://img.shields.io/nuget/dt/Mikodev.Binary.FSharp
