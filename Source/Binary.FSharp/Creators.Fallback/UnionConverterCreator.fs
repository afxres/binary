namespace Mikodev.Binary.Creators.Fallback

open Microsoft.FSharp.Reflection
open Mikodev.Binary
open Mikodev.Binary.Internal
open Mikodev.Binary.Internal.Contexts
open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Reflection

type internal UnionConverterCreator() =
    static let AllocatorByRefType = typeof<UnionEncoder<obj>>.GetMethod("Invoke").GetParameters().[0].ParameterType

    static let ReadOnlySpanByteByRefType = typeof<UnionDecoder<obj>>.GetMethod("Invoke").GetParameters().[0].ParameterType

    static let EncodeNumberMethodInfo = typeof<PrimitiveHelper>.GetMethod("EncodeNumber", [| AllocatorByRefType; typeof<int> |])

    static let DecodeNumberMethodInfo = typeof<PrimitiveHelper>.GetMethod("DecodeNumber", [| ReadOnlySpanByteByRefType |])

    static let GetEncodeMethodInfo (t : Type) (auto : bool) =
        let converterType = typedefof<Converter<_>>.MakeGenericType t
        let methodName = if auto then "EncodeAuto" else "Encode"
        let method = converterType.GetMethod(methodName, [| AllocatorByRefType; t |])
        method

    static let GetDecodeMethodInfo (t : Type) (auto : bool) =
        let converterType = typedefof<Converter<_>>.MakeGenericType t
        let methodName = if auto then "DecodeAuto" else "Decode"
        let method = converterType.GetMethod(methodName, [| ReadOnlySpanByteByRefType |])
        method

    static let GetEncodeExpression (t : Type) (converters : IReadOnlyDictionary<Type, IConverter>) (caseInfos : Map<int, UnionCaseInfo>) (tagMember : MemberInfo) (auto : bool) =
        let allocator = Expression.Parameter(AllocatorByRefType, "allocator")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let item = Expression.Parameter(t, "item")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (instance : Expression) (properties : PropertyInfo array) : Expression array = [|
            for i = 0 to properties.Length - 1 do
                let property = properties.[i]
                let propertyType = property.PropertyType
                let converter = converters.[propertyType]
                let method = GetEncodeMethodInfo propertyType (auto || i <> properties.Length - 1)
                let invoke = Expression.Call(Expression.Constant(converter), method, allocator, Expression.Property(instance, property))
                yield invoke
        |]

        let MakeCase (info : UnionCaseInfo) : Expression =
            let properties = info.GetFields()

            let MakeCastCase (dataType : Type) =
                let data = Expression.Variable(dataType, "data")
                let cast = Expression.Assign(data, Expression.Convert(item, dataType))
                let body = MakeBody data properties
                Expression.Block(Seq.singleton data, Array.append<Expression> [| cast |] body)

            if properties |> Array.isEmpty then
                Expression.Empty() :> Expression
            else
                let dataType = properties |> Array.map (fun x -> x.DeclaringType) |> Array.distinct |> Array.exactlyOne
                if t = dataType then
                    Expression.Block(MakeBody item properties) :> Expression
                else
                    MakeCastCase dataType :> Expression

        let switchCases =
            caseInfos
            |> Seq.map (fun x -> Expression.SwitchCase(MakeCase x.Value, Expression.Constant(x.Key)))
            |> Seq.toArray
        let tagExpression =
            match tagMember with
            | :? PropertyInfo as p -> Expression.Property(item, p) :> Expression
            | _ -> Expression.Call(tagMember :?> MethodInfo, item) :> Expression
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Empty())
        let block =
            Expression.Block(
                Seq.singleton flag,
                Expression.Assign(flag, tagExpression),
                Expression.Call(EncodeNumberMethodInfo, allocator, flag),
                Expression.Switch(flag, defaultBlock, switchCases))
        let delegateType = typedefof<UnionEncoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, allocator, item, mark)
        lambda

    static let GetDecodeExpression (t : Type) (converters : IReadOnlyDictionary<Type, IConverter>) (constructorInfos : Map<int, MethodInfo>) (auto : bool) =
        let span = Expression.Parameter(ReadOnlySpanByteByRefType, "span")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (constructor : MethodInfo) : Expression =
            let parameters = constructor.GetParameters()

            let MakeBodyItem () : Expression =
                let result = [|
                    for i = 0 to parameters.Length - 1 do
                        let parameterType = parameters.[i].ParameterType
                        let converter = converters.[parameterType]
                        let method = GetDecodeMethodInfo parameterType (auto || i <> parameters.Length - 1)
                        let variable = Expression.Variable(parameterType, sprintf "%d" i)
                        let invoke = Expression.Call(Expression.Constant(converter), method, span)
                        let assign = Expression.Assign(variable, invoke)
                        assign :> Expression, variable
                |]
                let expressions, variables = result |> Array.unzip
                let expressions = Array.append expressions [| Expression.Call(constructor, variables |> Seq.cast<Expression>) |]
                let block = Expression.Block(variables, expressions)
                block :> Expression

            if parameters |> Array.isEmpty then
                Expression.Call(constructor) :> Expression
            else
                MakeBodyItem ()

        let switchCases =
            constructorInfos
            |> Seq.map (fun x -> Expression.SwitchCase(MakeBody x.Value, Expression.Constant(x.Key)))
            |> Seq.toArray
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Default(t))
        let block =
            Expression.Block(
                Seq.singleton flag,
                Expression.Assign(flag, Expression.Call(DecodeNumberMethodInfo, span)),
                Expression.Switch(flag, defaultBlock, switchCases))
        let delegateType = typedefof<UnionDecoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, span, mark)
        lambda

    interface IConverterCreator with
        member __.GetConverter(context, t) =
            let Make (cases : UnionCaseInfo array) =
                let caseInfos = cases |> Array.map (fun x -> x.Tag, x) |> Map.ofArray
                let constructorInfos = cases |> Array.map (fun x -> x.Tag, FSharpValue.PreComputeUnionConstructorInfo(x)) |> Map.ofArray
                let memberTypes = seq {
                    for i in caseInfos do
                        for f in i.Value.GetFields() do
                            yield f.PropertyType
                    for i in constructorInfos do
                        for p in i.Value.GetParameters() do
                            yield p.ParameterType } |> Seq.distinct |> Seq.toArray
                let converters = memberTypes |> Array.map (fun x -> x, (Validate.GetConverter context x)) |> readOnlyDict
                let tagMember = FSharpValue.PreComputeUnionTagMemberInfo(t)
                let noNull = not t.IsValueType && not (tagMember :? MethodInfo)
                let encode = GetEncodeExpression t converters caseInfos tagMember false
                let decode = GetDecodeExpression t converters constructorInfos false
                let encodeAuto = GetEncodeExpression t converters caseInfos tagMember true
                let decodeAuto = GetDecodeExpression t converters constructorInfos true
                let converterType = typedefof<UnionConverter<_>>.MakeGenericType t
                let delegates = [| encode; decode; encodeAuto; decodeAuto |] |> Array.map (fun x -> x.Compile())
                let converterArguments = Array.append (delegates |> Seq.cast<obj> |> Seq.toArray) [| box noNull |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> IConverter

            if not (FSharpType.IsUnion(t)) || (t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>>) then
                null
            else
                let cases = FSharpType.GetUnionCases(t)
                if (cases |> Array.isEmpty) then
                    raise (ArgumentException(sprintf "No available union case found, type: %O" t))
                let unionType = cases |> Array.map (fun x -> x.DeclaringType) |> Array.distinct |> Array.exactlyOne
                if (unionType <> t) then
                    raise (ArgumentException(sprintf "Invalid union type, you may have to use union type '%O' instead of case type '%O'" unionType t))
                Make cases
