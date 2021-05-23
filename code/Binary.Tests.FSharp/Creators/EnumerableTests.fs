namespace Creators

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

type EnumerableTests () =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Assignable Interface Definitions`` () =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "FallbackCollectionMethods") |> Array.exactlyOne
        let f = t.GetField("ArrayOrListAssignableDefinitions", BindingFlags.Static ||| BindingFlags.NonPublic)
        let v = f.GetValue null :?> IReadOnlyList<Type>
        let arrayInterfaces = typeof<int array>.GetInterfaces()
        let listInterfaces = typeof<int ResizeArray>.GetInterfaces()
        let types = HashSet<_>()
        let listInterfaceGeneric = listInterfaces |> Array.filter (typeof<int seq>.IsAssignableFrom) in for i in listInterfaceGeneric do types.Add(i.GetGenericTypeDefinition()) |> ignore
        let arrayInterfacesGeneric = arrayInterfaces |> Array.filter (typeof<int seq>.IsAssignableFrom) in for i in arrayInterfacesGeneric do types.Add(i.GetGenericTypeDefinition()) |> ignore
        Assert.Equal<Type>(HashSet v, types)
        ()

    [<Fact>]
    member __.``IList (Array)`` () =
        let a = [| 1.2; 3.4; 5.6 |] :> IList<float>
        let bytes = generator.Encode a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.Decode<IList<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float array> value |> ignore
        ()

    [<Fact>]
    member __.``IList (Array Segment)`` () =
        let a = [| 9; 6; 3; |] |> ArraySegment
        let bytes = generator.Encode a
        Assert.Equal(12, bytes |> Array.length)
        let value = generator.Decode<IList<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int array> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyList`` () =
        let a = [ "some"; "times" ] |> ResizeArray :> IReadOnlyList<string>
        let bytes = generator.Encode a
        Assert.Equal(1 * 2 + 9, bytes |> Array.length)
        let value = generator.Decode<IReadOnlyList<string>> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string ResizeArray> value |> ignore
        ()

    [<Fact>]
    member __.``ICollection`` () =
        let a = [ 2.2; -4.5; 7.9 ] |> ResizeArray :> ICollection<float>
        let bytes = generator.Encode a
        Assert.Equal(24, bytes |> Array.length)
        let value = generator.Decode<ICollection<float>> bytes
        Assert.Equal<float>(a, value)
        Assert.IsType<float array> value |> ignore
        ()

    [<Fact>]
    member __.``IReadOnlyCollection`` () =
        let a = [| 13; 31; 131; 1313 |] :> IReadOnlyCollection<int>
        let bytes = generator.Encode a
        Assert.Equal(16, bytes |> Array.length)
        let value = generator.Decode<IReadOnlyCollection<int>> bytes
        Assert.Equal<int>(a, value)
        Assert.IsType<int array> value |> ignore
        ()

    [<Fact>]
    member __.``IEnumerable`` () =
        let a = seq { for i in 1..13 do yield sprintf "%x" i }
        let bytes = generator.Encode a
        let value = generator.Decode<string seq> bytes
        Assert.Equal<string>(a, value)
        Assert.IsType<string ResizeArray> value |> ignore
        ()
