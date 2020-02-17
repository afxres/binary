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

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| box [| 2; 6; 10 |] |]
            yield [| box [| "one"; "second"; "final" |] |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Fallback List Builder (hack, integration test)`` (array : 'a array) =
        let types = typeof<Converter>.Assembly.GetTypes()
        let listConverterCreator = types |> Array.filter (fun x -> x.Name = "ListConverterCreator") |> Array.exactlyOne
        let creatorTypes = types |> Array.filter (fun x -> not x.IsAbstract && typeof<IConverterCreator>.IsAssignableFrom x)
        let creators = creatorTypes |> Array.except (Array.singleton listConverterCreator) |> Array.map (fun x -> Activator.CreateInstance x :?> IConverterCreator)
        let generatorType = types |> Array.filter (fun x -> x.Name = "Generator" && not x.IsAbstract) |> Array.exactlyOne
        let generator = Activator.CreateInstance(generatorType, [| box Array.empty<Converter>; box creators |]) :?> IGenerator

        let alpha = generator.GetConverter<'a vlist>()
        Assert.Equal("EnumerableAdaptedConverter`2", alpha.GetType().Name)
        let defaultGenerator = Generator.CreateDefault()
        let bravo = defaultGenerator.GetConverter<'a vlist>()
        Assert.Equal("ArrayLikeAdaptedConverter`2", bravo.GetType().Name)

        let buffer = bravo.Encode (vlist array)
        Assert.Equal<byte>(buffer, alpha.Encode (vlist array))
        Assert.Equal<'a>(array, (alpha.Decode buffer).ToArray())
        Assert.Equal<'a>(array, (bravo.Decode buffer).ToArray())
        ()
