[<AutoOpen>]
module internal Mikodev.Binary.Internal.MemberHelper

open System

let MakeGenericType<'T> t =
    typeof<'T>.GetGenericTypeDefinition().MakeGenericType (Array.singleton t)

let IsImplementationOf<'T> (t : Type) =
    t.IsGenericType && t.GetGenericTypeDefinition() = typeof<'T>.GetGenericTypeDefinition()
