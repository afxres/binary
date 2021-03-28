namespace Creators

open Mikodev.Binary
open System
open System.Reflection
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

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| box [| 2; 6; 10 |] |]
        yield [| box [| "one"; "second"; "final" |] |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Fallback List Implementation (hack, integration test)`` (array : 'a array) =
        let generator = Generator.CreateDefault()
        let types = [ typeof<int>; typeof<string> ] |> List.map (fun x -> x, generator.GetConverter x) |> readOnlyDict
        let context = { new IGeneratorContext with member __.GetConverter t = types.[t] }

        let methodType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "FallbackCollectionMethods") |> Array.exactlyOne
        let method = methodType.GetMethod("GetConverter", BindingFlags.Static ||| BindingFlags.NonPublic, null, [| typeof<IGeneratorContext>; typeof<Type> |], null)

        let alpha = method.Invoke(null, [| box context; typeof<'a vlist> |]) :?> Converter<'a vlist>
        let alphaDecoder = alpha.GetType().GetField("decoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(alpha) |> unbox<Delegate>
        Assert.Contains("lambda", alphaDecoder.Method.Name)
        let bravo = generator.GetConverter<'a vlist>()
        let bravoBuilder = bravo.GetType().GetField("builder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue(bravo)
        Assert.Equal("ListBuilder`1", bravoBuilder.GetType().Name)

        let buffer = bravo.Encode (vlist array)
        Assert.Equal<byte>(buffer, alpha.Encode (vlist array))
        Assert.Equal<'a>(array, (alpha.Decode buffer).ToArray())
        Assert.Equal<'a>(array, (bravo.Decode buffer).ToArray())
        ()
