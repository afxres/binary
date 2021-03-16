[<AutoOpen>]
module internal Mikodev.Binary.Internal.CommonHelper

open System

let IsImplementationOf<'T> (t : Type) =
    t.IsGenericType && t.GetGenericTypeDefinition() = typeof<'T>.GetGenericTypeDefinition()

let MakeGenericType<'T> t =
    typeof<'T>.GetGenericTypeDefinition().MakeGenericType (Array.singleton t)
