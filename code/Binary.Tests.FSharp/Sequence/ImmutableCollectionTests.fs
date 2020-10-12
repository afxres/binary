namespace Sequence

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Reflection
open Xunit

type ImmutableCollectionTests() =
    let generator = Generator.CreateDefault()

    let TestNumberArray = [| 1; 5 |]

    let TestStringArray = [| "immutable"; "list"; "collection"; "readonly" |]

    let TestNumberStringPairArray = [| KeyValuePair(1, "one"); KeyValuePair(2, "two"); KeyValuePair(0, "zero") |]

    let TestStringNumberPairArray = [| KeyValuePair("Epsilon", Double.Epsilon); KeyValuePair("NaN", Double.NaN) |]

    let TestConverter (item : 'T when 'T :> 'E seq) =
        Assert.NotNull item
        let converter = generator.GetConverter<'T>()
        let converterType = converter.GetType()
        Assert.Equal("SequenceConverter`1", converterType.Name)
        let encoder = converterType.GetField("encoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter
        let encoderType = encoder.GetType()
        let encoderName =
            if typeof<'T>.IsInterface then
                if typeof<'T>.GetGenericArguments().Length = 1 then "EnumerableEncoder`2" else "KeyValueEnumerableEncoder`3"
            else
                "DelegateEncoder`1"
        Assert.Equal(encoderName, encoderType.Name)
        let decoder = converterType.GetField("decoder", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter
        Assert.Equal("DelegateDecoder`2", decoder.GetType().Name)
        converter

    let Test (item : 'T when 'T :> 'E seq) =
        let converter = TestConverter item
        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.Equal<'E>(item, result)
        ()

    let TestInterface (item : 'T when 'T :> 'E seq) =
        let converter = TestConverter item
        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.True(typeof<'T>.IsInterface)
        Assert.Equal<'E>(item, result)
        Assert.Equal(item.GetType(), result.GetType())

        let bufferDefault = converter.Encode null
        let resultDefault = converter.Decode bufferDefault
        Assert.Empty bufferDefault
        Assert.Empty resultDefault
        Assert.Equal(item.GetType(), resultDefault.GetType())
        ()

    let TestInvalid (item : 'T when 'T :> 'E seq) =
        Assert.Empty item
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter<'T>() |> ignore)
        let message = sprintf "Invalid collection type: %O" typeof<'T>
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``Immutable Array`` () =
        Test (ImmutableArray.CreateRange TestNumberArray)
        Test (ImmutableArray.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable HashSet`` () =
        Test (ImmutableHashSet.CreateRange TestNumberArray)
        Test (ImmutableHashSet.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable List`` () =
        Test (ImmutableList.CreateRange TestNumberArray)
        Test (ImmutableList.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable Queue`` () =
        Test (ImmutableQueue.CreateRange TestNumberArray)
        Test (ImmutableQueue.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable SortedSet`` () =
        Test (ImmutableSortedSet.CreateRange TestNumberArray)
        Test (ImmutableSortedSet.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable Dictionary`` () =
        Test (ImmutableDictionary.CreateRange TestNumberStringPairArray)
        Test (ImmutableDictionary.CreateRange TestStringNumberPairArray)
        ()

    [<Fact>]
    member __.``Immutable SortedDictionary`` () =
        Test (ImmutableSortedDictionary.CreateRange TestNumberStringPairArray)
        Test (ImmutableSortedDictionary.CreateRange TestStringNumberPairArray)
        ()

    [<Fact>]
    member __.``Interface Immutable List`` () =
        TestInterface (ImmutableList.CreateRange TestNumberArray :> IImmutableList<_>)
        TestInterface (ImmutableList.CreateRange TestStringArray :> IImmutableList<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Queue`` () =
        TestInterface (ImmutableQueue.CreateRange TestNumberArray :> IImmutableQueue<_>)
        TestInterface (ImmutableQueue.CreateRange TestStringArray :> IImmutableQueue<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Set`` () =
        TestInterface (ImmutableHashSet.CreateRange TestNumberArray :> IImmutableSet<_>)
        TestInterface (ImmutableHashSet.CreateRange TestStringArray :> IImmutableSet<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Dictionary`` () =
        TestInterface (ImmutableDictionary.CreateRange TestNumberStringPairArray :> IImmutableDictionary<_, _>)
        TestInterface (ImmutableDictionary.CreateRange TestStringNumberPairArray :> IImmutableDictionary<_, _>)
        ()

    [<Fact>]
    member __.``Invalid Immutable Stack`` () =
        TestInvalid ImmutableStack<int>.Empty
        TestInvalid ImmutableStack<string>.Empty
        ()

    [<Fact>]
    member __.``Invalid Interface Immutable Stack`` () =
        TestInvalid (ImmutableStack<int>.Empty :> IImmutableStack<_>)
        TestInvalid (ImmutableStack<string>.Empty :> IImmutableStack<_>)
        ()
