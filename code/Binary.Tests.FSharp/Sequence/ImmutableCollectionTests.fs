﻿namespace Sequence

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Reflection
open Xunit

type ImmutableCollectionTests() =
    let TestNumberArray = [| 1; 5 |]

    let TestStringArray = [| "immutable"; "list"; "collection"; "readonly" |]

    let TestNumberStringPairArray = [| KeyValuePair(1, "one"); KeyValuePair(2, "two"); KeyValuePair(0, "zero") |]

    let TestStringNumberPairArray = [| KeyValuePair("Epsilon", Double.Epsilon); KeyValuePair("NaN", Double.NaN) |]

    let TestInternalConverter =
        let g = Generator.CreateDefault()
        let context =
            { new IGeneratorContext with
                member __.GetConverter t =
                    Assert.Equal("System", t.Namespace)
                    g.GetConverter t
            }

        let t = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = "FallbackCollectionMethods") |> Array.exactlyOne
        let m = t.GetMethod("GetConverter", BindingFlags.Static ||| BindingFlags.NonPublic, null, [| typeof<IGeneratorContext>; typeof<Type> |], null)
        let d = Delegate.CreateDelegate(typeof<Func<Type, IConverter>>, context, m)
        d :?> Func<Type, IConverter>

    let TestConverter (item: 'T :> 'E seq) =
        Assert.NotNull item
        let converter = TestInternalConverter.Invoke typeof<'T> :?> Converter<'T>
        let converterType = converter.GetType()
        Assert.Equal("SequenceConverter`1", converterType.Name)
        let encoder = converterType.GetField("encode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
        let encoderMethod = encoder.Method
        if typeof<'T>.IsInterface then
            let encoderName =
                if typeof<'T>.GetGenericArguments().Length = 1 then
                    "EnumerableEncoder`2"
                else
                    "KeyValueEnumerableEncoder`3"
            Assert.Equal(encoderName, encoderMethod.DeclaringType.Name)
        else
            Assert.Null(encoderMethod.DeclaringType)
            Assert.Contains("lambda", encoder.Method.Name)
        let decoder = converterType.GetField("decode", BindingFlags.Instance ||| BindingFlags.NonPublic).GetValue converter |> unbox<Delegate>
        let decoderMethod = decoder.Method
        Assert.Null(decoderMethod.DeclaringType)
        Assert.Contains("lambda", decoder.Method.Name)
        converter

    let TestAutoAndLengthPrefix (item: 'T :> 'E seq) (converter: Converter<'T>) =
        let mutable allocatorAuto = Allocator()
        let mutable allocatorLengthPrefix = Allocator()
        converter.EncodeAuto(&allocatorAuto, item)
        converter.EncodeWithLengthPrefix(&allocatorLengthPrefix, item)
        let bufferAuto = allocatorAuto.ToArray()
        let bufferLengthPrefix = allocatorLengthPrefix.ToArray()
        Assert.Equal<byte>(bufferAuto, bufferLengthPrefix)

        let mutable spanAuto = ReadOnlySpan bufferAuto
        let mutable spanLengthPrefix = ReadOnlySpan bufferLengthPrefix
        let resultAuto = converter.DecodeAuto &spanAuto
        let resultLengthPrefix = converter.DecodeWithLengthPrefix &spanLengthPrefix
        Assert.Equal<'E>(item, resultAuto :> 'E seq)
        Assert.Equal<'E>(item, resultLengthPrefix :> 'E seq)
        Assert.Equal(0, spanAuto.Length)
        Assert.Equal(0, spanLengthPrefix.Length)
        ()

    let Test (item: 'T :> 'E seq) =
        let converter = TestConverter item
        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.Equal<'E>(item, result :> 'E seq)

        TestAutoAndLengthPrefix item converter
        ()

    let TestInterface (item: 'T :> 'E seq) =
        let converter = TestConverter item
        let buffer = converter.Encode item
        let result = converter.Decode buffer
        Assert.True(typeof<'T>.IsInterface)
        Assert.Equal<'E>(item, result :> 'E seq)
        Assert.Equal(item.GetType(), result.GetType())

        TestAutoAndLengthPrefix item converter

        let bufferDefault = converter.Encode null
        let resultDefault = converter.Decode bufferDefault
        Assert.Empty bufferDefault
        Assert.Empty resultDefault
        Assert.Equal(item.GetType(), resultDefault.GetType())

        TestAutoAndLengthPrefix resultDefault converter
        ()

    let TestInvalid (item: 'T :> 'E seq) =
        Assert.Empty item
        let error = Assert.Throws<ArgumentException>(fun () -> TestInternalConverter.Invoke typeof<'T> |> ignore)
        let message = sprintf "Invalid collection type: %O" typeof<'T>
        Assert.Null error.ParamName
        Assert.Equal(message, error.Message)
        ()

    [<Fact>]
    member __.``Immutable Array``() =
        Test(ImmutableArray.CreateRange TestNumberArray)
        Test(ImmutableArray.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable HashSet``() =
        Test(ImmutableHashSet.CreateRange TestNumberArray)
        Test(ImmutableHashSet.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable List``() =
        Test(ImmutableList.CreateRange TestNumberArray)
        Test(ImmutableList.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable Queue``() =
        Test(ImmutableQueue.CreateRange TestNumberArray)
        Test(ImmutableQueue.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable SortedSet``() =
        Test(ImmutableSortedSet.CreateRange TestNumberArray)
        Test(ImmutableSortedSet.CreateRange TestStringArray)
        ()

    [<Fact>]
    member __.``Immutable Dictionary``() =
        Test(ImmutableDictionary.CreateRange TestNumberStringPairArray)
        Test(ImmutableDictionary.CreateRange TestStringNumberPairArray)
        ()

    [<Fact>]
    member __.``Immutable SortedDictionary``() =
        Test(ImmutableSortedDictionary.CreateRange TestNumberStringPairArray)
        Test(ImmutableSortedDictionary.CreateRange TestStringNumberPairArray)
        ()

    [<Fact>]
    member __.``Interface Immutable List``() =
        TestInterface(ImmutableList.CreateRange TestNumberArray :> IImmutableList<_>)
        TestInterface(ImmutableList.CreateRange TestStringArray :> IImmutableList<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Queue``() =
        TestInterface(ImmutableQueue.CreateRange TestNumberArray :> IImmutableQueue<_>)
        TestInterface(ImmutableQueue.CreateRange TestStringArray :> IImmutableQueue<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Set``() =
        TestInterface(ImmutableHashSet.CreateRange TestNumberArray :> IImmutableSet<_>)
        TestInterface(ImmutableHashSet.CreateRange TestStringArray :> IImmutableSet<_>)
        ()

    [<Fact>]
    member __.``Interface Immutable Dictionary``() =
        TestInterface(ImmutableDictionary.CreateRange TestNumberStringPairArray :> IImmutableDictionary<_, _>)
        TestInterface(ImmutableDictionary.CreateRange TestStringNumberPairArray :> IImmutableDictionary<_, _>)
        ()

    [<Fact>]
    member __.``Invalid Immutable Stack``() =
        TestInvalid ImmutableStack<int>.Empty
        TestInvalid ImmutableStack<string>.Empty
        ()

    [<Fact>]
    member __.``Invalid Interface Immutable Stack``() =
        TestInvalid(ImmutableStack<int>.Empty :> IImmutableStack<_>)
        TestInvalid(ImmutableStack<string>.Empty :> IImmutableStack<_>)
        ()
