namespace Creators

open Mikodev.Binary
open System
open Xunit

type ListTests () =
    let generator = Generator.CreateDefault()

    [<Fact(DisplayName = "List")>]
    member __.``List`` () =
        let a = [ 1; 4; 7 ] |> vlist
        let b = [ "lazy"; "dog"; "quick"; "fox" ] |> vlist
        let bytesA = generator.Encode a
        let bytesB = generator.Encode b
        Assert.Equal(12, bytesA |> Array.length)
        Assert.Equal(1 * 4 + 15, bytesB |> Array.length)
        let valueA = generator.Decode<int vlist> bytesA
        let valueB = generator.Decode<string vlist> bytesB
        Assert.Equal<int>(a, valueA)
        Assert.Equal<string>(b, valueB)
        ()

    [<Fact(DisplayName = "List (null and empty)")>]
    member __.``List (null and empty)`` () =
        let a = Array.empty<int> |> vlist
        let b = null : string vlist
        let bytesA = generator.Encode a
        let bytesB = generator.Encode b
        Assert.NotNull(bytesA)
        Assert.NotNull(bytesB)
        Assert.Empty(bytesA)
        Assert.Empty(bytesB)
        let valueA = generator.Decode<int vlist> bytesA
        let valueB = generator.Decode<string vlist> bytesB
        Assert.Empty(valueA)
        Assert.Empty(valueB)
        ()

    [<Fact>]
    member __.``Fallback List Builder (hack)`` () =
        let findType name = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = name) |> Array.exactlyOne

        let builderDefinition = findType "ListFallbackBuilder`1"
        let builderType = builderDefinition.MakeGenericType(typeof<int>)
        let builder = Activator.CreateInstance builderType
        let count = builderType.GetMethod("Count").Invoke(builder, [| box (ReadOnlyMemory [| 1; 2 |]) |]) |> unbox<int>
        Assert.Equal(2, count)
        let get = builderType.GetMethod("Of")
        let emptyMemory = get.Invoke(builder, [| null |]) |> unbox<ReadOnlyMemory<int>>
        Assert.True(emptyMemory.IsEmpty)
        let memory = get.Invoke(builder, [| box (vlist [| 2; 4; 8; 16; 32 |]) |]) |> unbox<ReadOnlyMemory<int>>
        Assert.Equal<int>([| 2; 4; 8; 16; 32 |], memory.ToArray())

        let arrayLikeConverterDefinition = findType "ArrayLikeAdaptedConverter`2"
        let intConverter = generator.GetConverter<int>()
        let arrayLikeConverterType = arrayLikeConverterDefinition.MakeGenericType(typeof<int vlist>, typeof<int>)
        let arrayLikeConverterConstructor = arrayLikeConverterType.GetConstructors() |> Array.exactlyOne
        let arrayLikeConverter = arrayLikeConverterConstructor.Invoke([| box intConverter; box builder |]) :?> Converter<int vlist>

        let bufferOfNull = arrayLikeConverter.Encode null
        Assert.NotNull(bufferOfNull)
        Assert.Empty(bufferOfNull)
        let emptyListResult = arrayLikeConverter.Decode Array.empty<byte>
        Assert.NotNull(emptyListResult)
        Assert.Empty(emptyListResult)
        let listResult = arrayLikeConverter.Decode (generator.Encode [| 3; 6; 9 |])
        Assert.Equal<int>([| 3; 6; 9 |], listResult.ToArray())
        ()
