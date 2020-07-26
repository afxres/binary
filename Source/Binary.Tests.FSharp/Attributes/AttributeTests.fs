namespace Attributes

open Mikodev.Binary
open Mikodev.Binary.Attributes
open System
open System.Collections.Generic
open System.Reflection
open System.Text
open Xunit

type Int32AsStringConverter() =
    inherit Converter<int>()

    override __.Encode(allocator, item) =
        let text = string item
        let span = text.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, span)

    override __.Decode (span : inref<ReadOnlySpan<byte>>) =
        let text = Encoding.UTF8.GetString(span.ToArray())
        let item = Int32.Parse(text)
        item

type Int64AsStringConverter() =
    inherit Converter<int64>()

    override __.Encode(allocator, item) =
        let text = string item
        let span = text.AsSpan()
        PrimitiveHelper.EncodeString(&allocator, span)

    override __.Decode (span : inref<ReadOnlySpan<byte>>) =
        let text = Encoding.UTF8.GetString(span.ToArray())
        let item = Int64.Parse(text)
        item

type Int64AsStringConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(context, t) =
            Assert.NotNull(context)
            let converter = context.GetConverter(typeof<int64>)
            Assert.Equal(typeof<int64>, ConverterHelper.GetGenericArgument(converter))
            Assert.Equal(typeof<int64>, t)
            new Int64AsStringConverter() :> IConverter

type BadConverter<'T>() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (span : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type BadConverterWithoutPublicConstructor<'T> private() =
    inherit Converter<'T>()

    override __.Encode(_, _) = raise (NotSupportedException())

    override __.Decode (span : inref<ReadOnlySpan<byte>>) : 'T = raise (NotSupportedException())

type BadConverterCreatorWithoutPublicConstructor private() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = raise (NotSupportedException())

type BadConverterCreator<'T>() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = new BadConverter<'T>() :> IConverter

type NullConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = null

type AnotherNullConverterCreator() =
    interface IConverterCreator with
        member __.GetConverter(_, _) = null

// types

[<NamedObject>]
type ClassAsNamedObjectWithKey(head : double, next : string, tail : decimal) =
    [<NamedKey("OneOfThree")>]
    member __.Head = head

    [<NamedKey("2/3")>]
    member __.Next = next

    [<NamedKey("Last one")>]
    member __.Tail = tail

    override __.GetHashCode() = raise (NotSupportedException())

    override __.Equals obj =
        match obj with
        | :? ClassAsNamedObjectWithKey as other -> other.Head = head && other.Next = next && other.Tail = tail
        | _ -> false

[<NamedObject>]
type ClassAsNamedObjectWithPartiallyKey(first : string, second : float32, dayOfWeek : DayOfWeek) =
    member val IgnoreA = 0s with get, set

    [<NamedKey("1")>]
    member __.First = first

    member val IgnoreBravo = 1.1F with get, set

    [<NamedKey("sec")>]
    member __.Second = second

    [<NamedKey("day of WEEK")>]
    member __.DayOfWeek = dayOfWeek

    override __.GetHashCode() = raise (NotSupportedException())

    override me.Equals obj =
        match obj with
        | :? ClassAsNamedObjectWithPartiallyKey as other ->
            other.IgnoreA = me.IgnoreA && other.IgnoreBravo = me.IgnoreBravo &&
            other.First = first && other.Second = second && other.DayOfWeek = dayOfWeek
        | _ -> false

[<TupleObject>]
type ClassAsTupleObjectWithKey(beta : int64, release : Uri) =
    [<TupleKey(0)>]
    member __.Beta = beta

    [<TupleKey(1)>]
    member __.Release = release

    override __.GetHashCode() = raise (NotSupportedException())

    override __.Equals obj =
        match obj with
        | :? ClassAsTupleObjectWithKey as other -> other.Beta = beta && other.Release = release
        | _ -> false

[<TupleObject>]
type ClassAsTupleObjectWithPartiallyKey(preview : bool, debug : string, error : int) =
    [<TupleKey(2)>]
    member __.Preview = preview

    member __.What = 2.0F

    [<TupleKey(0)>]
    member __.Error = error

    [<TupleKey(1)>]
    member __.Debug = debug

    override __.GetHashCode() = raise (NotSupportedException())

    override __.Equals obj =
        match obj with
        | :? ClassAsTupleObjectWithPartiallyKey as other -> other.Preview = preview && other.Debug = debug && other.Error = error
        | _ -> false

[<TupleObject>]
type ClassAsTupleObjectWithUnorderedKey() =
    member val Ignore = 0.0 with get, set

    [<TupleKey(1)>]
    member val Alpha = 0 with get, set

    [<TupleKey(0)>]
    member val Candidate = String.Empty with get, set

    override __.GetHashCode() = raise (NotSupportedException())

    override me.Equals obj =
        match obj with
        | :? ClassAsTupleObjectWithUnorderedKey as other -> other.Alpha = me.Alpha && other.Candidate = me.Candidate
        | _ -> false

[<TupleObject>]
type ClassAsTupleObjectWithCustomConverterOfProperty() =
    [<TupleKey(0)>]
    member val X = 0 with get, set

    [<TupleKey(1)>]
    [<Converter(typeof<Int32AsStringConverter>)>]
    member val Y = 0 with get, set

    override __.GetHashCode() = raise (NotSupportedException())

    override me.Equals obj =
        match obj with
        | :? ClassAsTupleObjectWithCustomConverterOfProperty as other -> other.X = me.X && other.Y = me.Y
        | _ -> false

[<Struct>]
[<StructuralEquality>]
[<StructuralComparison>]
[<TupleObject>]
type ValueAsTupleObject =
    val mutable private x : float

    val mutable private y : float

    [<TupleKey(0)>]
    member me.X with get () = me.x and set item = me.x <- item

    [<TupleKey(1)>]
    member me.Y with get () = me.y and set item = me.y <- item

[<Struct>]
[<StructuralEquality>]
[<StructuralComparison>]
[<TupleObject>]
type ValueAsTupleObjectWithCustomConverterCreatorOfProperty(alpha : int64, beta : int64) =
    [<TupleKey(0)>]
    [<ConverterCreator(typeof<Int64AsStringConverterCreator>)>]
    member __.Alpha = alpha

    [<TupleKey(1)>]
    member __.Beta = beta

// bad types

[<NamedObject>]
type ClassAsNamedObjectWithoutKey() =
    member val Alpha = 0 with get, set

[<TupleObject>]
type ClassAsTupleObjectWithoutKey() =
    member val Alpha = 0 with get, set

[<NamedObject>]
[<TupleObject>]
type ClassAsWhat01() =
    member val What01 = 0 with get, set

[<NamedObject>]
[<Converter(typeof<BadConverter<ClassAsWhat02>>)>]
type ClassAsWhat02() =
    member val What02 = String.Empty with get, set

[<Converter(typeof<BadConverter<ClassAsWhat03>>)>]
[<ConverterCreator(typeof<BadConverterCreator<ClassAsWhat03>>)>]
type ClassAsWhat03() =
    member val What03 = 0.0 with get, set

[<TupleObject>]
[<ConverterCreator(typeof<BadConverterCreator<ClassAsWhat04>>)>]
type ClassAsWhat04() =
    member val What04 = 0.0F with get, set

[<Converter(typeof<BadConverter<int64>>)>]
type ClassWithBadConverter() =
    member val Random = 0uy with get, set

[<NamedObject>]
type ClassAsNamedObjectWithBadConverterOnProperty() =
    [<NamedKey("Some")>]
    [<Converter(typeof<BadConverter<string>>)>]
    member val Some = 0 with get, set

[<ConverterCreator(typeof<NullConverterCreator>)>]
type ClassWithNullConverterCreator() =
    member val None = String.Empty with get, set

[<TupleObject>]
type ClassAsTupleObjectWithNullConverterCreatorOnProperty() =
    [<TupleKey(0)>]
    [<ConverterCreator(typeof<AnotherNullConverterCreator>)>]
    member val Data = 0L with get, set

[<ConverterCreator(typeof<BadConverterCreator<int16>>)>]
type ClassWithBadConverterCreator() =
    member val Keyboard = 'k' with get, set

[<TupleObject>]
type ClassAsTupleObjectWithBadConverterCreatorOnProperty(x : double) =
    [<TupleKey(0)>]
    [<ConverterCreator(typeof<BadConverterCreator<single>>)>]
    member __.X = x

[<NamedObject>]
type ClassWithDuplicateNamedKey01() =
    [<NamedKey("alpha")>]
    member val Alpha = 0 with get, set

    [<NamedKey("bravo")>]
    member val Beta = 2.0 with get, set

    [<NamedKey("bravo")>]
    member val Charlie = "char" with get, set

[<NamedObject>]
type ClassWithDuplicateNamedKey02() =
    [<NamedKey("one")>]
    member val One = int16 1 with get, set

    [<NamedKey("2.0")>]
    member val Two = 2 with get, set

    member val What = "what the" with get, set

    [<NamedKey("2.0")>]
    member val Three = 3.0 with get, set

    [<NamedKey("one")>]
    member val First = 1.0F with get, set

[<TupleObject>]
type ClassWithDuplicateTupleKey01() =
    [<TupleKey(0)>]
    member val Maybe = true with get, set

    [<TupleKey(1)>]
    member val None = 0 with get, set

    [<TupleKey(0)>]
    member val IO = "io" with get, set

[<TupleObject>]
type ClassWithDuplicateTupleKey02() =
    [<TupleKey(1)>]
    member val Functor = "~f" with get, set

    [<TupleKey(2)>]
    member val Applicable = 3uy with get, set

    [<TupleKey(0)>]
    member val Overflow = -1 with get, set

    [<TupleKey(2)>]
    member val Panic = Exception() with get, set

[<TupleObject>]
type ClassWithNoSequentialTupleKey01() =
    [<TupleKey(1)>]
    member val A = 0 with get, set

    [<TupleKey(2)>]
    member val B = 1 with get, set

    [<TupleKey(3)>]
    member val C = 2 with get, set

[<TupleObject>]
type ClassWithNoSequentialTupleKey02() =
    [<TupleKey(-1)>]
    member val A = 0 with get, set

    [<TupleKey(0)>]
    member val B = 1 with get, set

    [<TupleKey(1)>]
    member val C = 2 with get, set

[<TupleObject>]
type ClassWithNoSequentialTupleKey03() =
    [<TupleKey(0)>]
    member val After = 0 with get, set

    [<TupleKey(1)>]
    member val Bee = 1.0 with get, set

    [<TupleKey(4)>]
    member val Char = "two" with get, set

[<TupleObject>]
type ClassWithNoSequentialTupleKey04() =
    member val Hidden = false with get, set

    [<TupleKey(2)>]
    member val Hello = 0L with get, set

    [<TupleKey(1)>]
    member val World = 1.0F with get, set

    [<TupleKey(4)>]
    member val My = 0 with get, set

    [<TupleKey(0)>]
    member val Computer = "pc"

[<NamedObject>]
type ClassWithMultipleAttributesOnProperty01() =
    [<NamedKey("value")>]
    [<Converter(typeof<BadConverter<int>>)>]
    [<ConverterCreator(typeof<BadConverterCreator<int>>)>]
    member val Value = 0 with get, set

[<NamedObject>]
type ClassWithMultipleAttributesOnProperty02() =
    [<TupleKey(0)>]
    [<NamedKey("data")>]
    member val Data = 0 with get, set

type ClassWithOnlyConverterAttribute() =
    [<Converter(typeof<BadConverter<string>>)>]
    member val Item = "item" with get, set

type ClassWithOnlyConverterCreatorAttribute() =
    [<ConverterCreator(typeof<BadConverterCreator<byte array>>)>]
    member val Collection = Array.empty<byte> with get, set

type ClassMissAttribute01() =
    [<NamedKey("class")>]
    member val Class = 0 with get, set

type ClassMissAttribute02() =
    [<TupleKey(0)>]
    member val Value = 0.0 with get, set

[<NamedObject>]
type ClassMissAttribute03() =
    [<TupleKey(1)>]
    member val Empty = "zero" with get, set

[<TupleObject>]
type ClassMissAttribute04() =
    [<NamedKey("error")>]
    member val Throw = "zero" with get, set

[<NamedObject>]
type ClassAsNamedObjectWithNullOrEmptyKey01() =
    [<NamedKey(null)>]
    member val Null = 0 with get, set

[<NamedObject>]
type ClassAsNamedObjectWithNullOrEmptyKey02() =
    [<NamedKey("")>]
    member val Empty = String.Empty with get, set

[<Converter(typeof<int>)>]
type ClassWithBadConverterAttribute01() =
    class
    end

[<Converter(null)>]
type ClassWithBadConverterAttribute02() =
    class
    end

[<NamedObject>]
type ClassWithBadConverterAttribute03() =
    [<NamedKey("alpha")>]
    [<Converter(typeof<BadConverterWithoutPublicConstructor<char>>)>]
    member val Alpha = 'a' with get, set

[<ConverterCreator(null)>]
type ClassWithBadConverterCreatorAttribute01() =
    class
    end

[<ConverterCreator(typeof<BadConverterCreatorWithoutPublicConstructor>)>]
type ClassWithBadConverterCreatorAttribute02() =
    class
    end

[<TupleObject>]
type ClassWithBadConverterCreatorAttribute03() =
    [<TupleKey(0)>]
    [<ConverterCreator(typeof<Guid>)>]
    member val Delta = 0.0 with get, set

[<Struct>]
[<StructuralEquality>]
[<NoComparison>]
[<TupleObject>]
type Tuple01<'T>(item01 : 'T) =
    [<TupleKey(0)>]
    member __.Item01 = item01

[<Struct>]
[<StructuralEquality>]
[<NoComparison>]
[<TupleObject>]
type Tuple02<'T1, 'T2>(item01 : 'T1, item02 : 'T2) =
    [<TupleKey(0)>]
    member __.Item01 = item01

    [<TupleKey(1)>]
    member __.Item02 = item02

[<Struct>]
[<StructuralEquality>]
[<NoComparison>]
[<TupleObject>]
type Tuple03<'T1, 'T2, 'T3>(item01 : 'T1, item02 : 'T2, item03 : 'T3) =
    [<TupleKey(0)>]
    member __.Item01 = item01

    [<TupleKey(1)>]
    member __.Item02 = item02

    [<TupleKey(2)>]
    member __.Item03 = item03

type AttributeTests() =
    let generator = Generator.CreateDefault()

    [<Fact>]
    member __.``Attributes All Public & Sealed & Not Allow Multiple & Not Inherited`` () =
        let assemblyTypes = typeof<IConverter>.Assembly.GetTypes()
        let types = assemblyTypes |> Array.where (fun x -> x.Namespace <> null && x.Namespace.EndsWith("Attributes") && x.IsSubclassOf(typeof<Attribute>))
        Assert.Equal(6, types.Length)
        let map = Dictionary<Type, AttributeTargets>()
        for i in types do
            Assert.True(i.IsPublic)
            Assert.True(i.IsSealed)
            let attribute = i.GetCustomAttribute<AttributeUsageAttribute>()
            Assert.False(attribute.Inherited)
            Assert.False(attribute.AllowMultiple)
            map.Add(i, attribute.ValidOn)
        let a = AttributeTargets.Property
        let b = AttributeTargets.Class ||| AttributeTargets.Struct ||| AttributeTargets.Enum ||| AttributeTargets.Interface
        let c = AttributeTargets.Class ||| AttributeTargets.Struct ||| AttributeTargets.Enum ||| AttributeTargets.Interface ||| AttributeTargets.Property
        Assert.Equal(a, map.[typeof<NamedKeyAttribute>])
        Assert.Equal(a, map.[typeof<TupleKeyAttribute>])
        Assert.Equal(b, map.[typeof<NamedObjectAttribute>])
        Assert.Equal(b, map.[typeof<TupleObjectAttribute>])
        Assert.Equal(c, map.[typeof<ConverterAttribute>])
        Assert.Equal(c, map.[typeof<ConverterCreatorAttribute>])
        ()

    static member ``Data Alpha`` : (obj array) seq =
        seq {
            yield [| new ClassAsNamedObjectWithKey(1.1, "second", 5.5m); [| "Last one", box 5.5m; "OneOfThree", box 1.1;  "2/3", box "second" |] |]
            yield [| new ClassAsNamedObjectWithPartiallyKey("f", -0.2F, DayOfWeek.Sunday); [| "sec", box -0.2F; "day of WEEK", box DayOfWeek.Sunday; "1", box "f" |] |]
        }

    static member ``Data Bravo`` : (obj array) seq =
        seq {
            yield [| new ClassAsTupleObjectWithKey(513L, new Uri("ws://loopback")); (513L, new Uri("ws://loopback")); 0 |]
            yield [| new ClassAsTupleObjectWithPartiallyKey(false, "debugging", -33); struct (-33, "debugging", false); 0 |]
            yield [| new ClassAsTupleObjectWithUnorderedKey(Alpha = 257, Candidate = "overflow"); ("overflow", 257); 0 |]
            yield [| new ClassAsTupleObjectWithCustomConverterOfProperty(X = 99, Y = 1080); (99, "1080"); 0 |]
            yield [| new ValueAsTupleObject(X = 1.1, Y = 2.3); (1.1, 2.3); 16 |]
            yield [| new ValueAsTupleObjectWithCustomConverterCreatorOfProperty(-2L, 333L); ("-2", 333L); 0 |]
            yield [| Tuple01(Int32.MaxValue); Tuple.Create(Int32.MaxValue); 4 |]
            yield [| Tuple01("01"); Tuple.Create("01"); 0 |]
            yield [| Tuple02(1.0, 3M); struct (1.0, 3M); 24 |]
            yield [| Tuple02(3.14, "pi"); (3.14, "pi"); 0 |]
            yield [| Tuple03(1uy, 2s, 3.0F); (1uy, 2s, 3.0F); 7 |]
            yield [| Tuple03("e", -2, Double.Epsilon); struct ("e", -2, Double.Epsilon); 0 |]
        }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Named Object (encode, decode)`` (source : 'a, expected : (string * obj) array) =
        let converter = generator.GetConverter<'a>()
        Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
        Assert.Equal(0, converter.Length)

        let mutable allocator = Allocator()
        converter.Encode(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()

        let token = Token(generator, buffer |> ReadOnlyMemory)
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(expected.Length, dictionary.Count)
        for (k, v) in expected do
            let token = dictionary.[k]
            let value = token.As(v.GetType())
            Assert.Equal(v, value)

        let result = converter.Decode(buffer)
        Assert.Equal<'a>(source, result)
        Assert.Equal(source.ToString(), result.ToString())
        ()

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Named Object (encode auto, decode auto)`` (source : 'a, expected : (string * obj) array) =
        let converter = generator.GetConverter<'a>()
        Assert.StartsWith("NamedObjectConverter`1", converter.GetType().Name)
        Assert.Equal(0, converter.Length)

        let mutable allocator = Allocator()
        converter.EncodeAuto(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()
        let mutable data = ReadOnlySpan buffer
        let data = PrimitiveHelper.DecodeBufferWithLengthPrefix &data

        let token = Token(generator, data.ToArray() |> ReadOnlyMemory)
        let dictionary = token :> IReadOnlyDictionary<string, Token>
        Assert.Equal(expected.Length, dictionary.Count)
        for (k, v) in expected do
            let token = dictionary.[k]
            let value = token.As(v.GetType())
            Assert.Equal(v, value)

        let mutable span = new ReadOnlySpan<byte>(buffer);
        let result = converter.DecodeAuto &span
        Assert.True(span.IsEmpty)
        Assert.Equal<'a>(source, result)
        Assert.Equal(source.ToString(), result.ToString())
        ()

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Tuple Object (encode, decode)`` (source : 'a, expected : 'b, size : int) =
        let converter = generator.GetConverter<'a>()
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name)
        Assert.Equal(size, converter.Length)

        let mutable allocator = Allocator()
        converter.Encode(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()

        let middle = generator.Decode<'b>(buffer)
        Assert.Equal<'b>(expected, middle)

        let result = converter.Decode(buffer)
        Assert.Equal<'a>(source, result)
        Assert.Equal(source.ToString(), result.ToString())
        ()

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Tuple Object (encode auto, decode auto)`` (source : 'a, expected : 'b, size : int) =
        let converter = generator.GetConverter<'a>()
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name)
        Assert.Equal(size, converter.Length)

        let mutable allocator = Allocator()
        converter.EncodeAuto(&allocator, source)
        let buffer = allocator.AsSpan().ToArray()

        let middleConverter = generator.GetConverter<'b>()
        let mutable middleSpan = new ReadOnlySpan<byte>(buffer);
        let middle = middleConverter.DecodeAuto &middleSpan
        Assert.True(middleSpan.IsEmpty)
        Assert.Equal<'b>(expected, middle)

        let mutable span = new ReadOnlySpan<byte>(buffer);
        let result = converter.DecodeAuto &span
        Assert.True(span.IsEmpty)
        Assert.Equal<'a>(source, result)
        Assert.Equal(source.ToString(), result.ToString())
        ()

    [<Theory>]
    [<InlineData(typeof<ClassAsNamedObjectWithoutKey>, "NamedKeyAttribute", "NamedObjectAttribute")>]
    [<InlineData(typeof<ClassAsTupleObjectWithoutKey>, "TupleKeyAttribute", "TupleObjectAttribute")>]
    member __.``Without key`` (t : Type, required : string, existed : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Require '%s' for '%s', type: %O" required existed t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassAsWhat01>)>]
    [<InlineData(typeof<ClassAsWhat02>)>]
    [<InlineData(typeof<ClassAsWhat03>)>]
    [<InlineData(typeof<ClassAsWhat04>)>]
    member __.``Multiple Attributes On Type`` (t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        Assert.Equal(sprintf "Multiple attributes found, type: %O" t, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithBadConverter>, typeof<ClassWithBadConverter>, typeof<BadConverter<int64>>)>]
    [<InlineData(typeof<ClassAsNamedObjectWithBadConverterOnProperty>, typeof<int>, typeof<BadConverter<string>>)>]
    member __.``Converter Attribute With Invalid Converter`` (t : Type, expectedType : Type, converterType : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Invalid custom converter '%O', expected converter item type: %O" converterType expectedType
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithNullConverterCreator>, typeof<ClassWithNullConverterCreator>, typeof<NullConverterCreator>)>]
    [<InlineData(typeof<ClassAsTupleObjectWithNullConverterCreatorOnProperty>, typeof<int64>, typeof<AnotherNullConverterCreator>)>]
    member __.``Converter Creator Attribute With Null Converter`` (t : Type, expectedType : Type, creatorType : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Invalid return value 'null', creator type: %O, expected converter item type: %O" creatorType expectedType
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithBadConverterCreator>, typeof<ClassWithBadConverterCreator>, typeof<BadConverter<int16>>, typeof<BadConverterCreator<int16>>)>]
    [<InlineData(typeof<ClassAsTupleObjectWithBadConverterCreatorOnProperty>, typeof<double>, typeof<BadConverter<single>>, typeof<BadConverterCreator<single>>)>]
    member __.``Converter Creator Attribute With Invalid Converter`` (t : Type, expectedType : Type, converterType : Type, creatorType : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Invalid custom converter '%O', creator type: %O, expected converter item type: %O" converterType creatorType expectedType
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithDuplicateNamedKey01>, "Charlie", "bravo")>]
    [<InlineData(typeof<ClassWithDuplicateNamedKey02>, "One", "one")>]
    member __.``Named Key Duplicated`` (t : Type, propertyName : string, key : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Named key '%s' already exists, property name: %s, type: %O" key propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithDuplicateTupleKey01>, "Maybe", 0)>]
    [<InlineData(typeof<ClassWithDuplicateTupleKey02>, "Panic", 2)>]
    member __.``Tuple Key Duplicated`` (t : Type, propertyName : string, key : int) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Tuple key '%d' already exists, property name: %s, type: %O" key propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithNoSequentialTupleKey01>)>]
    [<InlineData(typeof<ClassWithNoSequentialTupleKey02>)>]
    [<InlineData(typeof<ClassWithNoSequentialTupleKey03>)>]
    [<InlineData(typeof<ClassWithNoSequentialTupleKey04>)>]
    member __.``Tuple Key Not Sequential`` (t : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Tuple key must be start from zero and must be sequential, type: %O" t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithMultipleAttributesOnProperty01>, "Value")>]
    [<InlineData(typeof<ClassWithMultipleAttributesOnProperty02>, "Data")>]
    member __.``Multiple Attributes On Property`` (t : Type, propertyName : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Multiple attributes found, property name: %s, type: %O" propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithOnlyConverterAttribute>, "ConverterAttribute", "Item")>]
    [<InlineData(typeof<ClassWithOnlyConverterCreatorAttribute>, "ConverterCreatorAttribute", "Collection")>]
    member __.``Require Key Attribute`` (t : Type, attributeName : string, propertyName : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Require 'NamedKeyAttribute' or 'TupleKeyAttribute' for '%s', property name: %s, type: %O" attributeName propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassMissAttribute01>, "NamedObjectAttribute", "NamedKeyAttribute", "Class")>]
    [<InlineData(typeof<ClassMissAttribute02>, "TupleObjectAttribute", "TupleKeyAttribute", "Value")>]
    [<InlineData(typeof<ClassMissAttribute03>, "NamedKeyAttribute", "NamedObjectAttribute", "Empty")>]
    [<InlineData(typeof<ClassMissAttribute04>, "TupleKeyAttribute", "TupleObjectAttribute", "Throw")>]
    member __.``Require Attribute`` (t: Type, required : string, existed : string, propertyName : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Require '%s' for '%s', property name: %s, type: %O" required existed propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassAsTupleObjectWithKey>)>]
    [<InlineData(typeof<ClassAsTupleObjectWithPartiallyKey>)>]
    member __.``Tuple Object Null`` (t : Type) =
        let converter = generator.GetConverter t |> box :?> IConverter
        let alpha = Assert.Throws<ArgumentNullException>(fun () -> let mutable allocator = Allocator() in converter.Encode(&allocator, null))
        let bravo = Assert.Throws<ArgumentNullException>(fun () -> let mutable allocator = Allocator() in converter.EncodeAuto(&allocator, null))
        let message = sprintf "Tuple can not be null, type: %O" t
        Assert.Equal("item", alpha.ParamName)
        Assert.StartsWith(message, alpha.Message)
        Assert.Equal("item", bravo.ParamName)
        Assert.StartsWith(message, bravo.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<NamedKeyAttribute>, "Key")>]
    [<InlineData(typeof<ConverterAttribute>, "Type")>]
    [<InlineData(typeof<ConverterCreatorAttribute>, "Type")>]
    member __.``Create Attribute With Null`` (t : Type, propertyName : string) =
        let attribute = Activator.CreateInstance(t, Array.singleton<obj> null)
        let property = t.GetProperty(propertyName)
        let propertyValue = property.GetValue(attribute)
        Assert.Null(propertyValue)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassAsNamedObjectWithNullOrEmptyKey01>, "Null")>]
    [<InlineData(typeof<ClassAsNamedObjectWithNullOrEmptyKey02>, "Empty")>]
    member __.``Name Key Null Or Empty`` (t : Type, propertyName : string) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Named key can not be null or empty, property name: %s, type: %O" propertyName t
        Assert.Equal(message, error.Message)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithBadConverterAttribute01>, typeof<ClassWithBadConverterAttribute01>, typeof<InvalidCastException>)>]
    [<InlineData(typeof<ClassWithBadConverterAttribute02>, typeof<ClassWithBadConverterAttribute02>, typeof<ArgumentNullException>)>]
    [<InlineData(typeof<ClassWithBadConverterAttribute03>, typeof<char>, typeof<MissingMethodException>)>]
    member __.``Converter Attribute With Invalid Type`` (t : Type, expectedType : Type, errorType : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Can not get custom converter by attribute, expected converter item type: %O" expectedType
        Assert.Equal(message, error.Message)
        let innerError = error.InnerException
        Assert.IsType(errorType, innerError)
        ()

    [<Theory>]
    [<InlineData(typeof<ClassWithBadConverterCreatorAttribute01>, typeof<ClassWithBadConverterCreatorAttribute01>, typeof<ArgumentNullException>)>]
    [<InlineData(typeof<ClassWithBadConverterCreatorAttribute02>, typeof<ClassWithBadConverterCreatorAttribute02>, typeof<MissingMethodException>)>]
    [<InlineData(typeof<ClassWithBadConverterCreatorAttribute03>, typeof<double>, typeof<InvalidCastException>)>]
    member __.``Converter Creator Attribute With Invalid Type`` (t : Type, expectedType : Type, errorType : Type) =
        let error = Assert.Throws<ArgumentException>(fun () -> generator.GetConverter(t) |> ignore)
        let message = sprintf "Can not get custom converter creator by attribute, expected converter item type: %O" expectedType
        Assert.Equal(message, error.Message)
        let innerError = error.InnerException
        Assert.IsType(errorType, innerError)
        ()
