namespace Endianness

open Mikodev.Binary
open System
open System.Reflection
open Xunit

type ConverterTests() =
    [<Fact>]
    member __.``Half Converter`` () =
        let types = typeof<IConverter>.Assembly.GetTypes()
        let value = types |> Array.filter (fun x -> x.Name = "FallbackEndiannessMethods") |> Array.exactlyOne
        let method = value.GetMethods(BindingFlags.Static ||| BindingFlags.NonPublic) |> Array.filter (fun x -> x.ReturnType = typeof<IConverter> && x.Name.Contains("Invoke")) |> Array.exactlyOne
        let invoke = fun x -> method.Invoke(null, [| box typeof<Half>; box x |]) :?> Converter<Half>
        let native = invoke true
        let little = invoke false
        Assert.StartsWith("NativeEndianConverter`1", native.GetType().Name)
        Assert.StartsWith("LittleEndianConverter`1", little.GetType().Name)
        Assert.Equal(2, native.Length)
        Assert.Equal(2, little.Length)
        ()
