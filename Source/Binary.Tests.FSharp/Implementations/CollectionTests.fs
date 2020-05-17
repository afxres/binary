namespace Implementations

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Reflection
open Xunit

type CollectionTests() =

    [<Theory>]
    [<InlineData(4, 4, 1)>]
    [<InlineData(12, 6, 2)>]
    member __.``Capacity (valid)`` (byteLength : int, itemLength : int, expectedCapacity : int) =
        let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GenericsMethods") |> Array.exactlyOne
        let m = t.GetMethod("GetCapacity", BindingFlags.Static ||| BindingFlags.NonPublic)
        let f = Delegate.CreateDelegate(typeof<Func<int, int, Type, int>>, m) :?> Func<int, int, Type, int>
        let capacity = f.Invoke(byteLength, itemLength, typeof<int>)
        Assert.Equal(expectedCapacity, capacity)
        ()

    [<Theory>]
    [<InlineData(17, 4)>]
    [<InlineData(23, 8)>]
    member __.``Capacity (invalid)`` (byteLength : int, itemLength : int) =
        let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GenericsMethods") |> Array.exactlyOne
        let m = t.GetMethod("GetCapacity", BindingFlags.Static ||| BindingFlags.NonPublic)
        let f = Delegate.CreateDelegate(typeof<Func<int, int, Type, int>>, m) :?> Func<int, int, Type, int>
        let error = Assert.Throws<ArgumentException>(fun () -> f.Invoke(byteLength, itemLength, typeof<int>) |> ignore)
        let message = sprintf "Invalid collection bytes, byte count: %d, remainder: %d, item type: %O" byteLength (byteLength % itemLength) (typeof<int>)
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Empty`` : (obj array) seq =
        seq {
            yield [| Array.empty<int> |]
            yield [| Array.empty<string> |]
        }

    [<Theory>]
    [<MemberData("Data Empty")>]
    member __.``Contents (empty)`` (collection : ICollection<'a>) =
        let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GenericsMethods") |> Array.exactlyOne
        let m = t.GetMethod("GetContents", BindingFlags.Static ||| BindingFlags.NonPublic).MakeGenericMethod(typeof<'a>)
        let f = Delegate.CreateDelegate(typeof<Func<ICollection<'a>, 'a array>>, m) :?> Func<ICollection<'a>, 'a array>
        let result = f.Invoke collection
        Assert.True(obj.ReferenceEquals(Array.Empty<'a>(), result))
        ()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| [| 1; 2; 8 |] |]
            yield [| [| "e"; "r"; "r"; "o"; "r" |] |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Contents (not empty)`` (collection : ICollection<'a>) =
        let t = typeof<Converter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "GenericsMethods") |> Array.exactlyOne
        let m = t.GetMethod("GetContents", BindingFlags.Static ||| BindingFlags.NonPublic).MakeGenericMethod(typeof<'a>)
        let f = Delegate.CreateDelegate(typeof<Func<ICollection<'a>, 'a array>>, m) :?> Func<ICollection<'a>, 'a array>
        let result = f.Invoke collection
        Assert.NotEqual(0, result.Length)
        Assert.Equal<'a>(collection, result)
        ()
