module Sequence.EnumerableEncodeTests

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

let TestType (collection : 'T) (steps : 'T -> string seq) =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<'T>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`1", converterType.Name)
    let encoder = converterType.GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
    Assert.Null(encoder.Method.DeclaringType)
    Assert.Contains("lambda", encoder.Method.Name)
    Assert.Empty(steps collection)
    converter

let TestNull (collection : 'T when 'T :> FakeEnumerable<'E>) =
    let converter = TestType collection (fun x -> x.Steps :> _)
    Assert.Empty collection.Steps
    let nullInstance = Unchecked.defaultof<'T>
    Assert.Null nullInstance
    let buffer = converter.Encode nullInstance
    Assert.Empty buffer
    Assert.True(obj.ReferenceEquals(Array.Empty<byte>(), buffer))
    Assert.Empty collection.Steps
    ()

let Test (collection : 'T when 'T :> FakeEnumerable<'E>) (expected : string seq) =
    let converter = TestType collection (fun x -> x.Steps :> _)
    let steps = collection.Steps
    Assert.Empty steps
    converter.Encode collection |> ignore
    Assert.Equal<string>(expected, steps)
    ()

let TestValue (collection : 'T) (expected : string seq) (steps : 'T -> string seq) =
    let converter = TestType collection steps
    Assert.Empty(steps collection)
    converter.Encode collection |> ignore
    Assert.Equal<string>(expected, steps collection)
    ()

type FakeValueTypeEnumerator<'T> =
    struct
        val Steps : ResizeArray<string>

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

type FakeValueTypeEnumeratorValueSource<'T> =
    struct
        val Steps : ResizeArray<string>
    end

    new (steps) = { Steps = steps }

    interface IEnumerable<'T> with
        member __.GetEnumerator(): IEnumerator = raise (NotSupportedException())

        member __.GetEnumerator(): IEnumerator<'T> = raise (NotSupportedException())

    member me.GetEnumerator() = FakeValueTypeEnumerator<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator Can Dispose`` () =
    let source = FakeValueTypeEnumeratorSource<int>()
    Test source [ "start"; "current"; "end"; "dispose" ]
    ()

[<Fact>]
let ``Encode Custom Value Type Enumerator Can Dispose Value Type Source`` () =
    let source = FakeValueTypeEnumeratorValueSource<int>(ResizeArray<_>())
    TestValue source [ "start"; "current"; "end"; "dispose" ] (fun x -> x.Steps :> _)
    ()

[<Fact>]
let ``Encode Custom Value Type Enumerator Can Dispose For Null Instance`` () =
    TestNull (FakeValueTypeEnumeratorSource<int>())
    ()

type FakeValueTypeEnumeratorNoDispose<'T> =
    struct
        val Steps : ResizeArray<string>

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

[<Fact>]
let ``Encode Custom Value Type Enumerator No 'Dispose' For Null Instance`` () =
    TestNull (FakeValueTypeEnumeratorNoDisposeSource<int>())
    ()

type FakeValueTypeEnumeratorMultipleOverload<'T> =
    struct
        val Steps : ResizeArray<string>

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

[<Fact>]
let ``Encode Custom Value Type Enumerator With Bad 'Dispose' And Overload Methods For Null Instance`` () =
    TestNull (FakeValueTypeEnumeratorMultipleOverloadSource<int>())
    ()

let TestInvalid (collection : 'T when 'T :> FakeEnumerable<'E>) =
    let generator = Generator.CreateDefault()
    let converter = generator.GetConverter<'T>()
    let converterType = converter.GetType()
    Assert.Equal("SequenceConverter`1", converterType.Name)
    let encoder = converterType.GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
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
let ``Encode Custom Value Type Invalid Enumerator 'MoveNext' Return Type Mismatch`` () =
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
let ``Encode Custom Value Type Invalid Enumerator 'Current' Property Type Mismatch`` () =
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
let ``Encode Custom Value Type Invalid Enumerator 'Current' Property No Getter Or Is Indexer`` () =
    TestInvalid (FakeValueTypeEnumeratorCurrentPropertySignatureMismatchSource<uint64>())
    ()

type FakeValueTypeEnumeratorMoveNextThrowException<'T> =
    struct
        val Steps : ResizeArray<string>

        new (steps) = { Steps = steps }

        member __.MoveNext() : bool = raise (Exception("Error!"))

        member __.Current : 'T = raise (Exception("Unknown"))

        member me.Dispose() : unit = me.Steps.Add "Dispose"
    end

type FakeValueTypeEnumeratorMoveNextThrowExceptionSource<'T>() =
    inherit FakeEnumerable<'T>()

    member me.GetEnumerator() = FakeValueTypeEnumeratorMoveNextThrowException<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator 'MoveNext' Throw Exception`` () =
    let source = FakeValueTypeEnumeratorMoveNextThrowExceptionSource<int>()
    let converter = TestType source (fun x -> x.Steps :> _)
    let error = Assert.Throws<Exception>(fun () -> converter.Encode source |> ignore)
    Assert.Equal("Error!", error.Message)
    Assert.Equal<string>([| "Dispose" |], source.Steps)
    ()

[<Fact>]
let ``Encode Custom Value Type Enumerator 'MoveNext' Throw Exception For Null Instance`` () =
    TestNull (FakeValueTypeEnumeratorMoveNextThrowExceptionSource<int>())
    ()

type FakeValueTypeEnumeratorCurrentThrowException<'T> =
    struct
        val Steps : ResizeArray<string>

        new (steps) = { Steps = steps }

        member me.MoveNext() : bool = me.Steps.Add "MoveNext"; true

        member __.Current : 'T = raise (Exception("Unknown!"))

        member me.Dispose() : unit = me.Steps.Add "Dispose"
    end

type FakeValueTypeEnumeratorCurrentThrowExceptionSource<'T>() =
    inherit FakeEnumerable<'T>()

    member me.GetEnumerator() = FakeValueTypeEnumeratorCurrentThrowException<'T> me.Steps

[<Fact>]
let ``Encode Custom Value Type Enumerator 'Current' Throw Exception`` () =
    let source = FakeValueTypeEnumeratorCurrentThrowExceptionSource<int>()
    let converter = TestType source (fun x -> x.Steps :> _)
    let error = Assert.Throws<Exception>(fun () -> converter.Encode source |> ignore)
    Assert.Equal("Unknown!", error.Message)
    Assert.Equal<string>([| "MoveNext"; "Dispose" |], source.Steps)
    ()

[<Fact>]
let ``Encode Custom Value Type Enumerator 'Current' Throw Exception For Null Instance`` () =
    TestNull (FakeValueTypeEnumeratorCurrentThrowExceptionSource<int>())
    ()
