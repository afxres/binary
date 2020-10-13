module Sequence.EnumerationTests

open Mikodev.Binary
open System
open System.Collections
open System.Collections.Generic
open System.Reflection
open Xunit

type FakeEnumerable<'T>() =
    member val Steps = ResizeArray<string>()

    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = raise (NotSupportedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException())

let Test (collection : 'T when 'T :> FakeEnumerable<'E>) (expected : string seq) =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<'T>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`1", converterType.Name)
    let encoder = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter
    Assert.Equal("DelegateEncoder`1", encoder.GetType().Name)

    let steps = collection.Steps
    Assert.Empty steps
    converter.Encode collection |> ignore
    Assert.Equal<string>(expected, steps)
    ()

type FakeValueTypeEnumerator<'T> =
    struct
        val mutable Steps : ResizeArray<string>

        new (steps) = { Steps = steps }

        member me.MoveNext() : bool =
            if (me.Steps.Count = 0) then
                me.Steps.Add "start"
                true
            else
                me.Steps.Add "end"
                false

        member me.Current with get () =
            me.Steps.Add "current"
            Unchecked.defaultof<'T>

        member me.Dispose() =
            me.Steps.Add "dispose"
            ()
    end

type FakeValueTypeEnumeratorSource<'T>() =
    inherit FakeEnumerable<'T>()

    member me.GetEnumerator() = FakeValueTypeEnumerator<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator`` () =
    let source = FakeValueTypeEnumeratorSource<int>()
    Test source [ "start"; "current"; "end"; "dispose" ]
    ()

type FakeValueTypeEnumeratorNoDispose<'T> =
    struct
        val mutable Steps : ResizeArray<string>

        new (steps) = { Steps = steps }

        member me.MoveNext() : bool =
            if (me.Steps.Count = 0) then
                me.Steps.Add "start"
                true
            else
                me.Steps.Add "end"
                false

        member me.Current with get () =
            me.Steps.Add "current"
            Unchecked.defaultof<'T>
    end

type FakeValueTypeEnumeratorNoDisposeSource<'T>() =
    inherit FakeEnumerable<'T>()

    member me.GetEnumerator() = FakeValueTypeEnumeratorNoDispose<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator No Dispose`` () =
    let source = FakeValueTypeEnumeratorNoDisposeSource<int>()
    Test source [ "start"; "current"; "end" ]
    ()
