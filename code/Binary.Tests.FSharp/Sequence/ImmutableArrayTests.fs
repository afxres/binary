namespace Sequence

open Mikodev.Binary
open System.Collections.Immutable
open Xunit

type ImmutableArrayTests() =
    let generator = Generator.CreateDefault()

    member private __.TestConverter<'T>() =
        let converter = generator.GetConverter<ImmutableArray<'T>>()
        let t = converter.GetType()
        Assert.Matches("ArrayBased.*Converter`3", t.Name)
        converter

    [<Fact>]
    member me.``Converter Type Name And Length``() =
        let converter = me.TestConverter<int>()
        Assert.Equal(0, converter.Length)
        ()

    [<Fact>]
    member me.``Default Array``() =
        let source = ImmutableArray<int>()
        Assert.True source.IsDefault
        let converter = me.TestConverter<int>()
        let buffer = converter.Encode source
        Assert.Empty buffer
        let result = converter.Decode buffer
        Assert.False result.IsDefault
        Assert.Equal(0, result.Length)
        ()

    static member ``Data Alpha``: (obj array) seq = seq {
        yield [| Array.empty<string> |]
        yield [| [| -3; 1; 65565 |] |]
        yield [| [| ""; "alpha"; "bravo"; "charlie" |] |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member me.``Encode Decode``(values: 'T array) =
        let source = ImmutableArray.CreateRange values
        let converter = me.TestConverter<'T>()
        let buffer = converter.Encode source
        let result = converter.Decode buffer
        Assert.False result.IsDefault
        Assert.Equal<'T>(values, result)
        ()
