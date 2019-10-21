namespace Creators

open Mikodev.Binary
open Xunit

type ListTests () =
    let generator = GeneratorBuilder().AddDefaultConverterCreators().Build();

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

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| typeof<int vlist> |]
            yield [| typeof<string vlist> |]
        }
