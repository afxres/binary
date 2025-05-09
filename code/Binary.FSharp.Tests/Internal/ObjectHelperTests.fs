module Internal.ObjectHelperTests

open Mikodev.Binary
open System
open System.Collections.Generic
open System.Collections.Immutable
open System.Linq
open System.Reflection
open Xunit

type GetList<'T> = delegate of Converter<'T> * ReadOnlySpan<byte> -> 'T list

type GetPartialList<'T> = delegate of Converter<'T> * byref<ReadOnlySpan<byte>> -> obj

[<Literal>]
let MaxLevels = 32

[<Literal>]
let NewLength = 64

let GetMethod<'D> () =
    let arguments = typeof<'D>.GetGenericArguments()
    let t = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes().Single(fun x -> x.Name = "ObjectHelper" && x.Namespace = "Mikodev.Binary.Internal")
    let method = t.GetMethod(typeof<'D>.Name.Split('`').First(), BindingFlags.Static ||| BindingFlags.NonPublic)
    Assert.NotNull method
    let result = Delegate.CreateDelegate(typeof<'D>, method.MakeGenericMethod arguments) |> unbox<'D>
    result

let ``Get List Test Data``: seq<obj array> = seq {
    yield [| ImmutableArray.Create<int>(); 0; 0 |]
    yield [| ImmutableArray.Create<string>(); 0; 0 |]
    yield [| Enumerable.Range(0, 1).ToImmutableArray(); 1; 1 |]
    yield [| Enumerable.Range(0, 1).Select(string).ToImmutableArray(); 1; 1 |]
    yield [| Enumerable.Range(0, MaxLevels - 1).ToImmutableArray(); MaxLevels - 1; MaxLevels - 1 |]
    yield [| Enumerable.Range(0, MaxLevels - 1).Select(string).ToImmutableArray(); MaxLevels - 1; MaxLevels - 1 |]
    yield [| Enumerable.Range(0, MaxLevels).ToImmutableArray(); MaxLevels; MaxLevels |]
    yield [| Enumerable.Range(0, MaxLevels).Select(string).ToImmutableArray(); MaxLevels; MaxLevels |]
    yield [| Enumerable.Range(0, MaxLevels + 1).ToImmutableArray(); MaxLevels; NewLength |]
    yield [| Enumerable.Range(0, MaxLevels + 1).Select(string).ToImmutableArray(); MaxLevels; NewLength |]
    yield [| Enumerable.Range(0, MaxLevels + 127).ToImmutableArray(); MaxLevels; NewLength |]
    yield [| Enumerable.Range(0, MaxLevels + 127).Select(string).ToImmutableArray(); MaxLevels; NewLength |]
}

[<Theory>]
[<MemberData(nameof (``Get List Test Data``))>]
let ``Get List Test``<'E> (source: IReadOnlyCollection<'E>, countExpected: int, capacityExpected: int) =
    let getList = GetMethod<GetList<'E>>()
    let getPartialList = GetMethod<GetPartialList<'E>>()

    let generator = Generator.CreateDefault()
    let buffer = generator.Encode(source)
    let converter = generator.GetConverter<'E>()
    let result = getList.Invoke(converter, buffer)
    let mutable span = ReadOnlySpan<byte>(buffer)
    let resultPartial = getPartialList.Invoke(converter, &span)

    Assert.Equal(source.Count, result.Length)
    Assert.Equal<'E>(source, result)
    if source.Count <= MaxLevels then
        let resultPartial = unbox<'E list> resultPartial
        Assert.Equal(countExpected, resultPartial.Length)
        Assert.Equal(capacityExpected, resultPartial.Length)
        Assert.Equal<'E>(source, resultPartial)
    else
        let resultPartial = unbox<ResizeArray<'E>> resultPartial
        Assert.Equal(countExpected, resultPartial.Count)
        Assert.Equal(capacityExpected, resultPartial.Capacity)
        Assert.Equal<'E>(source.Take countExpected, resultPartial)
    ()
