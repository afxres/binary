module Contexts.AllocatorModuleTests

open Mikodev.Binary
open System
open System.Buffers
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Text
open Xunit

let allocatorType = typeof<IConverter>.Assembly.GetTypes() |> Array.filter (fun x -> x.IsValueType && x.Name = "Allocator") |> Array.exactlyOne

[<Fact>]
let ``Append Action (default constructor, length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); ())
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append Max Length (default constructor, max length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); -1)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append Max Length With Length Prefix (default constructor, max length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.AppendWithLengthPrefix(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); -1)
    Assert.Equal(1, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Action (default constructor, length invalid)`` (length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, length, null :> obj, fun a b -> raise (NotSupportedException()); ())
        ())
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "SpanAction`2") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters().[1]
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    Assert.Equal("length", parameter.Name)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Max Length (default constructor, max length invalid)`` (maxLength : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, maxLength, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "AllocatorWriter`1") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters().[1]
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    Assert.Equal("maxLength", parameter.Name)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Max Length With Length Prefix (default constructor, max length invalid)`` (maxLength : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, maxLength, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "AppendWithLengthPrefix" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "AllocatorWriter`1") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters().[1]
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    Assert.Equal("maxLength", parameter.Name)
    ()

[<Fact>]
let ``Append Action (default constructor, length overflow)`` () =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, Int32.MaxValue + 1, null :> obj, fun a b -> raise (NotSupportedException()); ())
        ())
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    ()

[<Fact>]
let ``Append Max Length (default constructor, max length overflow)`` () =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, Int32.MaxValue + 1, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    ()

[<Fact>]
let ``Append Max Length With Length Prefix (default constructor, max length overflow)`` () =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, Int32.MaxValue + 1, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    ()

[<Fact>]
let ``Append Action (append some then, length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, 8, null :> obj, fun a b -> ())
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); ())
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Fact>]
let ``Append Max Length (append some then, max length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, 8, null :> obj, fun a b -> (); 8)
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); -1)
    Assert.Equal(8, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Fact>]
let ``Append Max Length With Length Prefix (append some then, max length zero with raise expression)`` () =
    let mutable allocator = Allocator()
    Allocator.AppendWithLengthPrefix(&allocator, 8, null :> obj, fun a b -> (); 8)
    Assert.Equal(9, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    Allocator.AppendWithLengthPrefix(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); -1)
    Assert.Equal(10, allocator.Length)
    Assert.Equal(256, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Action (append some then, length invalid)`` (length : int) =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, 8, null :> obj, fun a b -> ())
        flag <- 1
        Allocator.Append(&allocator, length, null :> obj, fun a b -> raise (NotSupportedException()); ())
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    Assert.Equal("length", error.ParamName)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Max Length (append some then, max length invalid)`` (maxLength : int) =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, 8, null :> obj, fun a b -> (); 8)
        flag <- 1
        Allocator.Append(&allocator, maxLength, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    ()

[<Theory>]
[<InlineData(-1)>]
[<InlineData(-100)>]
[<InlineData(Int32.MinValue)>]
let ``Append Max Length With Length Prefix (append some then, max length invalid)`` (maxLength : int) =
    let mutable flag = 0
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, 8, null :> obj, fun a b -> (); 8)
        flag <- 1
        Allocator.AppendWithLengthPrefix(&allocator, maxLength, null :> obj, fun a b -> raise (NotSupportedException()); -1)
        ())
    Assert.Equal(1, flag)
    Assert.StartsWith("Argument max length must be greater than or equal to zero!", error.Message)
    Assert.Equal("maxLength", error.ParamName)
    ()

[<Fact>]
let ``Append Action (limited to zero, length zero with raise expression)`` () =
    let mutable allocator = Allocator(Span(), maxCapacity = 0)
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); ())
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append Max Length (limited to zero, max length zero with raise expression)`` () =
    let mutable allocator = Allocator(Span(), maxCapacity = 0)
    Allocator.Append(&allocator, 0, null :> obj, fun a b -> raise (NotSupportedException()); -1)
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Fact>]
let ``Append Action (1 byte 512 times, capacity test)`` () =
    let mutable allocator = Allocator()
    for item in 1..512 do
        Allocator.Append(&allocator, 1, null :> obj,
            fun (a : Span<byte>) (b : obj) ->
                Assert.Null b
                Assert.Equal(1, a.Length))
        Assert.Equal(item, allocator.Length)
        Assert.Equal((if item > 256 then 512 else 256), allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1, 256)>]
[<InlineData(256, 256)>]
[<InlineData(257, 512)>]
[<InlineData(666, 1024)>]
[<InlineData(1024, 1024)>]
let ``Append Action (default constructor)`` (length : int, capacityExpected : int) =
    let mutable allocator = Allocator()
    let buffer = Array.zeroCreate<byte> length
    Random.Shared.NextBytes buffer
    Allocator.Append(&allocator, length, buffer,
        fun (a : Span<byte>) (b : byte array) ->
            b.CopyTo a
            Assert.Equal(length, a.Length))
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(buffer, result)
    Assert.Equal(capacityExpected, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(32)>]
[<InlineData(256)>]
[<InlineData(768)>]
let ``Append Action (limited, overflow)`` (limits : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Span(), limits)
        Allocator.Append(&allocator, limits + 1, null :> obj, fun a b -> ()))
    Assert.Null(error.ParamName)
    Assert.Equal("Maximum capacity has been reached.", error.Message)
    ()

[<Fact>]
let ``Append Action (limited)`` () =
    let mutable allocator = Allocator(Span (Array.zeroCreate 96), 640)
    Allocator.Append(&allocator, 190, null :> obj, fun a b -> ())
    Assert.Equal(96 * 2, allocator.Capacity)
    Allocator.Append(&allocator, 450, null :> obj, fun a b -> ())
    Assert.Equal(640, allocator.Length)
    Assert.Equal(640, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Append Action (default constructor, action null)`` (length : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, length, null :> obj, Unchecked.defaultof<SpanAction<_, _>>))
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "SpanAction`2") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", parameter.Name)
    Assert.Equal("action", error.ParamName)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Append Max Length (default constructor, writer null)`` (maxLength : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, maxLength, null :> obj, Unchecked.defaultof<AllocatorWriter<_>>))
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Append" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "AllocatorWriter`1") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("writer", parameter.Name)
    Assert.Equal("writer", error.ParamName)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(4)>]
let ``Append Max Length With Length Prefix (default constructor, writer null)`` (maxLength : int) =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, maxLength, null :> obj, Unchecked.defaultof<AllocatorWriter<_>>))
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "AppendWithLengthPrefix" && x.GetParameters().Length = 4)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[3].ParameterType.Name.StartsWith "AllocatorWriter`1") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("writer", parameter.Name)
    Assert.Equal("writer", error.ParamName)
    ()

[<Theory>]
[<InlineData(1)>]
[<InlineData("data")>]
let ``Append Action (with data)`` (data : 'a) =
    let mutable flag : 'a option = None
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, 1, data, fun a b -> flag <- Some b)
    Assert.Equal<'a>(data, flag |> Option.get)
    ()

[<Fact>]
let ``Append Buffer (empty)`` () =
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, ReadOnlySpan (Array.empty))
    Assert.Equal(0, allocator.Length)
    Assert.Equal(0, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1, 256)>]
[<InlineData(256, 256)>]
[<InlineData(386, 512)>]
[<InlineData(512, 512)>]
[<InlineData(768, 1024)>]
[<InlineData(1024, 1024)>]
let ``Append Buffer (random)`` (length : int, capacityExpected : int) =
    let buffer = Array.zeroCreate<byte> length
    Random.Shared.NextBytes buffer
    let mutable allocator = Allocator()
    Allocator.Append(&allocator, ReadOnlySpan buffer)
    Assert.Equal(length, allocator.Length)
    Assert.Equal(capacityExpected, allocator.Capacity)
    ()

[<Fact>]
let ``Append With Length Prefix (action null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, Array.empty<byte>, null)
        ())
    let methodInfos = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "AppendWithLengthPrefix" && x.GetParameters().Length = 3)
    let methodInfo = methodInfos |> Array.filter (fun x -> x.GetParameters().[2].ParameterType.Name.StartsWith "AllocatorAction") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", parameter.Name)
    Assert.Equal("action", error.ParamName)
    ()

[<Theory>]
[<InlineData(0)>]
[<InlineData(1)>]
[<InlineData(256)>]
[<InlineData(65536)>]
let ``Append With Length Prefix`` (length : int) =
    let source = Array.zeroCreate length
    Random.Shared.NextBytes source
    let mutable allocator = Allocator()
    Allocator.AppendWithLengthPrefix<byte array>(&allocator, source, fun a b -> Allocator.Append(&a, ReadOnlySpan b))
    let mutable span = allocator.AsSpan()
    let result = Converter.DecodeWithLengthPrefix &span
    Assert.True(span.IsEmpty)
    Assert.Equal<byte>(source, result.ToArray())
    ()

[<Fact>]
let ``Allocator Action (contravariant)`` () =
    let t = typedefof<AllocatorAction<_>>
    let parameter = t.GetGenericArguments() |> Array.exactlyOne
    Assert.Equal(GenericParameterAttributes.Contravariant, parameter.GenericParameterAttributes)
    ()

[<Fact>]
let ``Allocator Writer (contravariant)`` () =
    let t = typedefof<AllocatorWriter<_>>
    let parameter = t.GetGenericArguments() |> Array.exactlyOne
    Assert.Equal(GenericParameterAttributes.Contravariant, parameter.GenericParameterAttributes)
    ()

[<Fact>]
let ``Invoke (action null)`` () =
    let error = Assert.Throws<ArgumentNullException>(fun () -> Allocator.Invoke(0, null) |> ignore)
    let methodInfo = allocatorType.GetMethods() |> Array.filter (fun x -> x.Name = "Invoke") |> Array.exactlyOne
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("action", error.ParamName)
    Assert.Equal("action", parameter.Name)
    ()

[<Fact>]
let ``Invoke (empty action)`` () =
    let mutable length = -1;
    let mutable capacity = -1;
    let mutable maxCapacity = -1
    let buffer = Allocator.Invoke(0, fun allocator _ ->
        length <- allocator.Length
        capacity <- allocator.Capacity
        maxCapacity <- allocator.MaxCapacity)
    Assert.Equal(0, length)
    Assert.Equal(capacity, 65536)
    Assert.Equal(Int32.MaxValue, maxCapacity)
    Assert.NotNull(buffer)
    Assert.Equal(0, buffer.Length)
    ()

[<Theory>]
[<InlineData("")>]
[<InlineData("Hello, 世界")>]
[<InlineData("一二三四五六七八九十")>]
let ``Invoke (encode some string with length prefix)`` (text : string) =
    let buffer = Allocator.Invoke(text, fun allocator item -> Allocator.AppendWithLengthPrefix(&allocator, item.AsSpan(), Encoding.UTF32))
    let mutable span = ReadOnlySpan<byte>(buffer)
    let result = Encoding.UTF32.GetString(Converter.DecodeWithLengthPrefix &span)
    Assert.Equal(text, result)
    Assert.Equal(0, span.Length)
    ()

[<Theory>]
[<InlineData(0, 0, -1)>]
[<InlineData(16, 0, Int32.MinValue)>]
[<InlineData(4096, 1024, -1)>]
[<InlineData(8192, 5120, Int32.MinValue)>]
let ``Ensure (negative)`` (maxCapacity : int, offset : int, length : int) =
    let error = Assert.Throws<ArgumentOutOfRangeException>(fun () ->
        let mutable allocator = Allocator(Span(), maxCapacity)
        Allocator.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
        Assert.Equal(offset, allocator.Length)
        Assert.Equal(maxCapacity, allocator.MaxCapacity)
        Allocator.Ensure(&allocator, length))
    let methodInfo = allocatorType.GetMethod("Ensure", BindingFlags.Static ||| BindingFlags.Public)
    let parameter = methodInfo.GetParameters() |> Array.last
    Assert.Equal("length", parameter.Name)
    Assert.Equal("length", error.ParamName)
    Assert.StartsWith("Argument length must be greater than or equal to zero!", error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 1)>]
[<InlineData(16, 0, 17)>]
[<InlineData(16, 12, 5)>]
[<InlineData(16, 16, 1)>]
[<InlineData(4096, 1024, 8192)>]
[<InlineData(8192, 5120, 3840)>]
let ``Ensure (invalid)`` (maxCapacity : int, offset : int, length : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Span(), maxCapacity)
        Allocator.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
        Assert.Equal(offset, allocator.Length)
        Assert.Equal(maxCapacity, allocator.MaxCapacity)
        Allocator.Ensure(&allocator, length))
    let message = "Maximum capacity has been reached."
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 0)>]
[<InlineData(16, 0, 0)>]
[<InlineData(16, 12, 0)>]
[<InlineData(16, 16, 0)>]
[<InlineData(16, 12, 4)>]
[<InlineData(4096, 0, 2048)>]
[<InlineData(4096, 1024, 2048)>]
[<InlineData(4096, 2048, 2048)>]
let ``Ensure (no resize)`` (capacity : int, offset : int, length : int) =
    let mutable allocator = Allocator(Span(Array.zeroCreate capacity), capacity)
    let origin = &MemoryMarshal.GetReference(allocator.AsSpan())
    Allocator.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &MemoryMarshal.GetReference(allocator.AsSpan())))

    Allocator.Ensure(&allocator, length)
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &MemoryMarshal.GetReference(allocator.AsSpan())))
    ()

[<Theory>]
[<InlineData(0, 0, 1)>]
[<InlineData(16, 0, 17)>]
[<InlineData(16, 12, 5)>]
[<InlineData(16, 16, 1)>]
[<InlineData(4096, 1024, 8192)>]
[<InlineData(8192, 5120, 3840)>]
let ``Expand (invalid)`` (maxCapacity : int, offset : int, length : int) =
    let error = Assert.Throws<ArgumentException>(fun () ->
        let mutable allocator = Allocator(Span(), maxCapacity)
        Allocator.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
        Assert.Equal(offset, allocator.Length)
        Assert.Equal(maxCapacity, allocator.MaxCapacity)
        Allocator.Expand(&allocator, length))
    let message = "Maximum capacity has been reached."
    Assert.Null error.ParamName
    Assert.Equal(message, error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 0)>]
[<InlineData(16, 0, 0)>]
[<InlineData(16, 12, 0)>]
[<InlineData(16, 16, 0)>]
[<InlineData(16, 12, 4)>]
[<InlineData(4096, 0, 2048)>]
[<InlineData(4096, 1024, 2048)>]
[<InlineData(4096, 2048, 2048)>]
let ``Expand (no resize)`` (capacity : int, offset : int, length : int) =
    let mutable allocator = Allocator(Span(Array.zeroCreate capacity), capacity)
    let origin = &MemoryMarshal.GetReference(allocator.AsSpan())
    Allocator.Append(&allocator, ReadOnlySpan(Array.zeroCreate offset))
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &MemoryMarshal.GetReference(allocator.AsSpan())))

    Allocator.Expand(&allocator, length)
    Assert.Equal(capacity, allocator.MaxCapacity)
    Assert.Equal(capacity, allocator.Capacity)
    Assert.Equal(offset + length, allocator.Length)
    Assert.True(Unsafe.AreSame(&origin, &MemoryMarshal.GetReference(allocator.AsSpan())))
    ()

[<Theory>]
[<InlineData(0, 0, 0)>]
[<InlineData(16, 0, 256)>]
[<InlineData(16, 16, 256)>]
[<InlineData(512, 128, 512)>]
[<InlineData(768, 512, 1024)>]
[<InlineData(4096, 4096, 4096)>]
let ``Append Max Length (integration test)`` (maxLength : int, actual : int, capacityExpected : int) =
    let mutable allocator = Allocator()
    let buffer = Array.zeroCreate actual
    Allocator.Append(&allocator, maxLength, buffer, fun (a : Span<byte>) (b : byte array) -> b.CopyTo a; b.Length)
    let result = allocator.AsSpan().ToArray()
    Assert.Equal<byte>(buffer, result)
    Assert.Equal(actual, allocator.Length)
    Assert.Equal(capacityExpected, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1, -1)>]
[<InlineData(16, -1)>]
[<InlineData(16, 17)>]
[<InlineData(384, -200)>]
[<InlineData(512, 768)>]
[<InlineData(512, Int32.MinValue)>]
let ``Append Max Length (invalid return value)`` (maxLength : int, actual : int) =
    let error = Assert.Throws<InvalidOperationException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.Append(&allocator, maxLength, null :> obj, fun a b -> (); actual)
        ())
    Assert.Equal("Invalid return value.", error.Message)
    ()

[<Theory>]
[<InlineData(0, 0, 1, 1, 256)>]
[<InlineData(127, 0, 1, 1, 256)>]
[<InlineData(128, 0, 4, 4, 256)>]
[<InlineData(16, 16, 1, 17, 256)>]
[<InlineData(56, 48, 1, 49, 256)>]
[<InlineData(384, 72, 4, 76, 512)>]
[<InlineData(1536, 96, 4, 100, 2048)>]
let ``Append Max Length With Length Prefix (integration test)`` (maxLength : int, actual : int, prefixLength : int, lengthExpected : int, capacityExpected : int) =
    let mutable allocator = Allocator()
    let buffer = Array.zeroCreate actual
    Allocator.AppendWithLengthPrefix(&allocator, maxLength, buffer, fun (a : Span<byte>) (b : byte array) -> b.CopyTo a; b.Length)
    let result = allocator.AsSpan().ToArray()
    let mutable span = ReadOnlySpan result
    let header = Converter.Decode &span
    Assert.Equal(actual, header)
    Assert.Equal(prefixLength, allocator.Length - span.Length)
    Assert.Equal(lengthExpected, allocator.Length)
    Assert.Equal<byte>(buffer, span.ToArray())
    Assert.Equal(actual, span.Length)
    Assert.Equal(capacityExpected, allocator.Capacity)
    ()

[<Theory>]
[<InlineData(1, -1)>]
[<InlineData(16, -1)>]
[<InlineData(16, 17)>]
[<InlineData(384, -200)>]
[<InlineData(512, 768)>]
[<InlineData(512, Int32.MinValue)>]
let ``Append Max Length With Length Prefix (invalid return value)`` (maxLength : int, actual : int) =
    let error = Assert.Throws<InvalidOperationException>(fun () ->
        let mutable allocator = Allocator()
        Allocator.AppendWithLengthPrefix(&allocator, maxLength, null :> obj, fun a b -> (); actual)
        ())
    Assert.Equal("Invalid return value.", error.Message)
    ()
