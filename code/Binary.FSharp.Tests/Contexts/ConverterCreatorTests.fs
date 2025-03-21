﻿namespace Contexts

open Mikodev.Binary
open System
open System.Collections.Generic
open Xunit

type FakeIConverterImplementation() =
    interface IConverter with
        member __.Decode(span: inref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.Decode(_: byte[]) : obj = raise (NotSupportedException())

        member __.DecodeAuto(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.DecodeWithLengthPrefix(span: byref<ReadOnlySpan<byte>>) : obj = raise (NotSupportedException())

        member __.Encode(_: obj) : byte[] = raise (NotSupportedException())

        member __.Encode(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.EncodeAuto(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.EncodeWithLengthPrefix(allocator: byref<Allocator>, _: obj) : unit = raise (NotSupportedException())

        member __.Length: int = raise (NotSupportedException())

type FakeIGeneratorContextImplementation(converters: IReadOnlyDictionary<Type, IConverter>) =
    interface IGeneratorContext with
        member __.GetConverter t =
            let flag, result = converters.TryGetValue t
            if flag then result else null

type FakeGenericConverterImplementation<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode(_: inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type ConverterCreatorTests() =
    static member ``Data Alpha``: (obj array) seq = seq {
        yield [| "UnionConverterCreator"; typeof<int option> |]
        yield [| "FSharpListConverterCreator"; typeof<int list> |]
        yield [| "FSharpMapConverterCreator"; typeof<Map<int, int>> |]
        yield [| "FSharpSetConverterCreator"; typeof<Set<int>> |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Invalid IConverter Implementation``(creatorName: string, itemType: Type) =
        let creatorType = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = creatorName) |> Array.exactlyOne
        let creator = Activator.CreateInstance creatorType :?> IConverterCreator
        let context =
            [| typeof<int>, FakeIConverterImplementation() :> IConverter |] |> readOnlyDict |> FakeIGeneratorContextImplementation :> IGeneratorContext
        let error = Assert.Throws<ArgumentException>(fun () -> creator.GetConverter(context, itemType) |> ignore)
        let message = sprintf "Can not convert '%O' to '%O'" typeof<FakeIConverterImplementation> typeof<Converter<int>>
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Invalid Converter Instance``(creatorName: string, itemType: Type) =
        let creatorType = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = creatorName) |> Array.exactlyOne
        let creator = Activator.CreateInstance creatorType :?> IConverterCreator
        let context =
            [| typeof<int>, FakeGenericConverterImplementation<string>() :> IConverter |] |> readOnlyDict |> FakeIGeneratorContextImplementation
            :> IGeneratorContext
        let error = Assert.Throws<ArgumentException>(fun () -> creator.GetConverter(context, itemType) |> ignore)
        let message = sprintf "Can not convert '%O' to '%O'" typeof<FakeGenericConverterImplementation<string>> typeof<Converter<int>>
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Invalid Null Converter Return``(creatorName: string, itemType: Type) =
        let creatorType = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes() |> Array.filter (fun x -> x.Name = creatorName) |> Array.exactlyOne
        let creator = Activator.CreateInstance creatorType :?> IConverterCreator
        let context = Array.empty |> readOnlyDict |> FakeIGeneratorContextImplementation :> IGeneratorContext
        let error = Assert.Throws<ArgumentException>(fun () -> creator.GetConverter(context, itemType) |> ignore)
        let message = sprintf "Can not convert null to '%O'" typeof<Converter<int>>
        Assert.Equal(message, error.Message)
        ()
