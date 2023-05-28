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
        let method = t.GetMethod(methodName, BindingFlags.Static ||| BindingFlags.NonPublic, null, parameterTypes, null)
        Assert.NotNull method
        Delegate.CreateDelegate(typeof<'T>, method) :?> 'T

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| typeof<HelperMethodTests>; "NotFound" |]
        yield [| typeof<HelperMethodTests>; "Function" |]
    }

    [<Theory>]
    [<MemberData(nameof HelperMethodTests.``Data Alpha``)>]
    member __.``Get Method Error`` (t : Type, methodName : string) =
        let invoke = HelperMethodTests.GetCommonHelperMethod<Func<Type, string, Type array, MethodInfo>> "GetMethod"
        let error = Assert.Throws<MissingMethodException>(fun () -> invoke.Invoke(t, methodName, Array.empty) |> ignore)
        let message = $"Method not found, method name: {methodName}, type: {t}"
        Assert.Equal(message, error.Message)
        ()
