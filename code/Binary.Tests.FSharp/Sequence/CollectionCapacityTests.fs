namespace Sequence

open Mikodev.Binary
open System
open System.Reflection
open Xunit

type CollectionCapacityTests() =
    [<Theory>]
    [<InlineData(4, 4, 1)>]
    [<InlineData(12, 6, 2)>]
    member __.``Capacity (valid)``(byteLength: int, itemLength: int, expectedCapacity: int) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "SequenceContext") |> Array.exactlyOne
        let m = t.GetMethod("GetCapacity", BindingFlags.Static ||| BindingFlags.NonPublic, null, [| typeof<int>; typeof<int> |], null)
        let f = Delegate.CreateDelegate(typeof<Func<int, int, int>>, m.MakeGenericMethod typeof<int>) :?> Func<int, int, int>
        let capacity = f.Invoke(byteLength, itemLength)
        Assert.Equal(expectedCapacity, capacity)
        ()

    [<Theory>]
    [<InlineData(17, 4)>]
    [<InlineData(23, 8)>]
    member __.``Capacity (invalid)``(byteLength: int, itemLength: int) =
        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "SequenceContext") |> Array.exactlyOne
        let m = t.GetMethod("GetCapacity", BindingFlags.Static ||| BindingFlags.NonPublic, null, [| typeof<int>; typeof<int> |], null)
        let f = Delegate.CreateDelegate(typeof<Func<int, int, int>>, m.MakeGenericMethod typeof<int>) :?> Func<int, int, int>
        let error = Assert.Throws<ArgumentException>(fun () -> f.Invoke(byteLength, itemLength) |> ignore)
        let message = sprintf "Not enough bytes for collection element, byte length: %d, element type: %O" byteLength typeof<int>
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()
