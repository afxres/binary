namespace Creators

open Mikodev.Binary
open System
open System.Linq
open System.Reflection
open Xunit

type ArrayTests () =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Array`` () =
        let a : int array = [| 1; 4; 16; |]
        let b : string array = [| "alpha"; "beta"; "release" |]
        let bytesA = generator.Encode a
        let bytesB = generator.Encode b
        Assert.Equal(12, bytesA.Length)
        Assert.Equal(1 * 3 + 5 + 4 + 7, bytesB.Length)
        let valueA = generator.Decode<int array> bytesA
        let valueB = generator.Decode<string array> bytesB
        Assert.Equal<int>(a, valueA)
        Assert.Equal<string>(b, valueB)
        ()

    [<Fact>]
    member __.``Array Of String (from 0 to 1024)`` () =
        let converter = generator.GetConverter<string array>()
        for i = 0 to 1024 do
            let source = Enumerable.Range(0, i) |> Seq.map string |> Seq.toArray
            let buffer = converter.Encode source
            let result = converter.Decode buffer
            Assert.Equal(i, result.Length)
            Assert.Equal<string>(source, result)
        ()

    [<Fact>]
    member __.``Array of Arrays`` () =
        let array = [| [| 1; 2|]; [| 5; 7; 9|] |]
        Assert.Equal(1, array.Rank)
        let bytes = generator.Encode array
        Assert.Equal(1 * 2 + 4 * 5, bytes |> Array.length)
        let value = generator.Decode<int [] []> bytes
        Assert.Equal<int []>(array, value)
        ()

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| typeof<int> |]
        yield [| typeof<double seq> |]
    }

    static member ``Data Bravo`` : (obj array) seq = seq {
        yield [| Array2D.zeroCreate<int> 2 3 |]
        yield [| Array.CreateInstance(typeof<int>, [| 3; 4 |], [| 1; 2 |]) |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Non Array Type`` (t : Type) =
        let methodType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "FallbackSequentialMethods") |> Array.exactlyOne
        let method = methodType.GetMethod("GetConverter", BindingFlags.Static ||| BindingFlags.NonPublic)
        let converter = method.Invoke(null, [| null; box t |])
        Assert.Null converter
        ()

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Non SZ Array`` (o : obj) =
        let methodType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "FallbackSequentialMethods") |> Array.exactlyOne
        let method = methodType.GetMethod("GetConverter", BindingFlags.Static ||| BindingFlags.NonPublic)
        let t = o.GetType()
        Assert.True t.IsArray
        let error = Assert.Throws<TargetInvocationException>(fun () -> method.Invoke(null, [| null; box t |]) |> ignore)
        let error = Assert.IsType<NotSupportedException> error.InnerException
        let message = sprintf "Only single dimensional zero based arrays are supported, type: %O" t
        Assert.Equal(message, error.Message)
        ()
