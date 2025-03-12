namespace Internal

open Mikodev.Binary
open System
open System.Reflection
open Xunit

type HelperMethodTests() =
    static member GetCommonHelperMethod<'T when 'T :> Delegate> methodName =
        let invoke = typeof<'T>.GetMethod("Invoke", BindingFlags.Instance ||| BindingFlags.Public)
        Assert.NotNull invoke
        let parameterTypes = invoke.GetParameters() |> Array.map (fun x -> x.ParameterType)
        let t = typeof<GeneratorBuilderFSharpExtensions>.Assembly.GetTypes() |> Seq.filter (fun x -> x.Name = "CommonHelper") |> Seq.exactlyOne
        let method = t.GetMethod(methodName, BindingFlags.Static ||| BindingFlags.Public ||| BindingFlags.NonPublic, null, parameterTypes, null)
        Assert.NotNull method
        Delegate.CreateDelegate(typeof<'T>, method) :?> 'T

    static member ``Data Type Invalid``: (obj array) seq = seq {
        yield [| typeof<IConverter>.Assembly; "NotExist" |]
        yield [| typeof<IConverter>.Assembly; "Argument" |]
    }

    [<Theory>]
    [<MemberData(nameof HelperMethodTests.``Data Type Invalid``)>]
    member __.``Get Type Error``(assembly: Assembly, typeName: string) =
        let invoke = HelperMethodTests.GetCommonHelperMethod<Func<Assembly, string, Type>> "GetType"
        let error = Assert.Throws<TypeLoadException>(fun () -> invoke.Invoke(assembly, typeName) |> ignore)
        let message = $"Type not found, type name: {typeName}"
        Assert.Equal(message, error.Message)
        ()

    static member ``Data Alpha``: (obj array) seq = seq {
        yield [| typeof<HelperMethodTests>; "NotFound" |]
        yield [| typeof<HelperMethodTests>; "Function" |]
    }

    [<Theory>]
    [<MemberData(nameof HelperMethodTests.``Data Alpha``)>]
    member __.``Get Method Error``(t: Type, methodName: string) =
        let invoke = HelperMethodTests.GetCommonHelperMethod<Func<Type, string, Type array, MethodInfo>> "GetMethod"
        let error = Assert.Throws<MissingMethodException>(fun () -> invoke.Invoke(t, methodName, Array.empty) |> ignore)
        let message = $"Method not found, method name: {methodName}, type: {t}"
        Assert.Equal(message, error.Message)
        ()
