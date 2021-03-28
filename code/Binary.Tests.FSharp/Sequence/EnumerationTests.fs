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
    let encoder = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    Assert.Null(encoder.Method.DeclaringType)
    Assert.Contains("lambda", encoder.Method.Name)

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
                me.Steps.Add "1"
                true
            else
                me.Steps.Add "3"
                false

        member me.Current with get () =
            me.Steps.Add "2"
            Unchecked.defaultof<'T>
    end

type FakeValueTypeEnumeratorNoDisposeSource<'T>() =
    inherit FakeEnumerable<'T>()

    member me.GetEnumerator() = FakeValueTypeEnumeratorNoDispose<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator No 'Dispose'`` () =
    let source = FakeValueTypeEnumeratorNoDisposeSource<int>()
    Test source [ "1"; "2"; "3" ]
    ()

type FakeValueTypeEnumeratorMultipleOverload<'T> =
    struct
        val mutable Steps : ResizeArray<string>

        new (steps) = { Steps = steps }

        member __.MoveNext(_a : obj) : bool = raise (NotSupportedException())

        member me.MoveNext() : bool =
            if (me.Steps.Count = 0) then
                me.Steps.Add "a"
                true
            else
                me.Steps.Add "c"
                false

        member __.MoveNext(_a : int, _b : string) : bool = raise (NotSupportedException())

        member me.Current with get () =
            me.Steps.Add "b"
            Unchecked.defaultof<'T>

        member __.Dispose(_a : string, _b : int) : Guid = raise (NotSupportedException())

        member __.Dispose() : string = raise (NotSupportedException())

        member __.Dispose(_a : obj) : unit = raise (NotSupportedException())
    end

type FakeValueTypeEnumeratorMultipleOverloadSource<'T>() =
    inherit FakeEnumerable<'T>()

    member __.GetEnumerator(_a : obj) = raise (NotSupportedException())

    member me.GetEnumerator() = FakeValueTypeEnumeratorMultipleOverload<'T> me.Steps

    member __.GetEnumerator(_a : int, _base : single) = raise (NotSupportedException())

[<Fact>]
let ``Encode Custom Value Type Enumerator With Bad 'Dispose' And Overload Methods`` () =
    let source = FakeValueTypeEnumeratorMultipleOverloadSource<int>()
    Test source [ "a"; "b"; "c" ]
    ()

let TestInvalid (collection : 'T when 'T :> FakeEnumerable<'E>) =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<'T>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`1", converterType.Name)
    let encoder = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    Assert.Equal("EnumerableEncoder`2", encoder.Method.DeclaringType.Name)
    Assert.Empty collection.Steps
    ()

type FakeValueTypeEnumeratorMoveNextReturnTypeMismatch<'T> =
    struct
        member __.MoveNext() : bool voption = raise (NotSupportedException())

        member __.Current with get () : 'T = raise (NotSupportedException())
    end

type FakeValueTypeEnumeratorMoveNextReturnTypeMismatchSource<'T>() =
    inherit FakeEnumerable<'T>()

    member __.GetEnumerator() = FakeValueTypeEnumeratorMoveNextReturnTypeMismatch<'T>()

[<Fact>]
let ``Encode Custom Value Type Enumerator 'MoveNext' Return Type Mismatch`` () =
    TestInvalid (FakeValueTypeEnumeratorMoveNextReturnTypeMismatchSource<single>())
    ()

type FakeValueTypeEnumeratorCurrentPropertyTypeMismatch<'T> =
    struct
        member __.MoveNext() : bool = raise (NotSupportedException())

        member __.Current with get () : 'T voption = raise (NotSupportedException())
    end

type FakeValueTypeEnumeratorCurrentPropertyTypeMismatchSource<'T>() =
    inherit FakeEnumerable<'T>()

    member __.GetEnumerator() = FakeValueTypeEnumeratorCurrentPropertyTypeMismatch<'T>()

[<Fact>]
let ``Encode Custom Value Type Enumerator 'Current' Property Type Mismatch`` () =
    TestInvalid (FakeValueTypeEnumeratorCurrentPropertyTypeMismatchSource<double>())
    ()

type FakeValueTypeEnumeratorCurrentPropertySignatureMismatch<'T> =
    struct
        member __.MoveNext() : bool = raise (NotSupportedException())

        member __.Current with set (_i : int) (_a : 'T) = raise (NotSupportedException())

        member __.Current with get (_a : int, _b : string) : 'T = raise (NotSupportedException())
    end

type FakeValueTypeEnumeratorCurrentPropertySignatureMismatchSource<'T>() =
    inherit FakeEnumerable<'T>()

    member __.GetEnumerator() = FakeValueTypeEnumeratorCurrentPropertySignatureMismatch<'T>()

[<Fact>]
let ``Encode Custom Value Type Enumerator 'Current' Property No Getter Or Is Indexer`` () =
    TestInvalid (FakeValueTypeEnumeratorCurrentPropertySignatureMismatchSource<uint64>())
    ()
