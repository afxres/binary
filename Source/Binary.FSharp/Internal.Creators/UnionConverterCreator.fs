namespace Mikodev.Binary.Internal.Creators

open Microsoft.FSharp.Reflection
open Mikodev.Binary
open System
open System.Linq.Expressions
open System.Reflection

type UnionConverterCreator() =
    static let AllocatorByRefType = typeof<UnionEncoder<obj>>.GetMethod("Invoke").GetParameters().[0].ParameterType

    static let ReadOnlySpanByteByRefType = typeof<UnionDecoder<obj>>.GetMethod("Invoke").GetParameters().[0].ParameterType

    static let EncodeNumberMethodInfo = typeof<PrimitiveHelper>.GetMethod("EncodeNumber", [| AllocatorByRefType; typeof<int> |])

    static let DecodeNumberMethodInfo = typeof<PrimitiveHelper>.GetMethod("DecodeNumber", [| ReadOnlySpanByteByRefType |])

    static let MakeConverterType (t : Type) = typedefof<Converter<_>>.MakeGenericType t

    static let GetEncodeMethodInfo (t : Type) (isAuto : bool) =
        let converterType = MakeConverterType t
        let methodName = if isAuto then "EncodeAuto" else "Encode"
        let method = converterType.GetMethod(methodName, [| AllocatorByRefType; t |])
        method

    static let GetDecodeMethodInfo (t : Type) (isAuto : bool) =
        let converterType = MakeConverterType t
        let methodName = if isAuto then "DecodeAuto" else "Decode"
        let method = converterType.GetMethod(methodName, [| ReadOnlySpanByteByRefType |])
        method

    static let GetEncodeExpression (context : IGeneratorContext) (t : Type) (caseInfos : UnionCaseInfo seq) (tagMember : MemberInfo) (isAuto : bool) =
        let allocator = Expression.Parameter(AllocatorByRefType, "allocator")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let item = Expression.Parameter(t, "item")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (instance : Expression) (properties : PropertyInfo array) : Expression array = [|
            for i = 0 to properties.Length - 1 do
                let property = properties.[i]
                let propertyType = property.PropertyType
                let converter = context.GetConverter propertyType
                let method = GetEncodeMethodInfo propertyType (isAuto || i <> properties.Length - 1)
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

            match properties with
            | null | [| |] -> Expression.Empty() :> Expression
            | _ ->
                let dataType = properties |> Array.map (fun x -> x.DeclaringType) |> Array.distinct |> Array.exactlyOne
                if t = dataType then
                    Expression.Block(MakeBody item properties) :> Expression
                else
                    MakeCastCase dataType :> Expression

        let switchCases =
            caseInfos
            |> Seq.map (fun x -> Expression.SwitchCase(MakeCase x, Expression.Constant(x.Tag)))
            |> Seq.toArray
        let tagExpression =
            match tagMember with
            | :? PropertyInfo as p -> Expression.Property(item, p) :> Expression
            | _ -> Expression.Call(tagMember :?> MethodInfo, item) :> Expression
        let setMethod = EncodeNumberMethodInfo
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Empty())
        let block =
            Expression.Block(
                Seq.singleton flag,
                Expression.Assign(flag, tagExpression),
                Expression.Call(setMethod, allocator, flag),
                Expression.Switch(flag, defaultBlock, switchCases))
        let delegateType = typedefof<UnionEncoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, allocator, item, mark)
        lambda

    static let GetDecodeExpression (context : IGeneratorContext) (t : Type) (caseInfos : UnionCaseInfo seq) (isAuto : bool) =
        let span = Expression.Parameter(ReadOnlySpanByteByRefType, "span")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (info : UnionCaseInfo) : Expression =
            let constructor = FSharpValue.PreComputeUnionConstructorInfo(info)
            let parameters = constructor.GetParameters()

            let MakeBodyItem () : Expression =
                let result = [|
                    for i = 0 to parameters.Length - 1 do
                        let parameterType = parameters.[i].ParameterType
                        let converter = context.GetConverter parameterType
                        let method = GetDecodeMethodInfo parameterType (isAuto || i <> parameters.Length - 1)
                        let variable = Expression.Variable(parameterType, sprintf "%d" i)
                        let invoke = Expression.Call(Expression.Constant(converter), method, span)
                        let assign = Expression.Assign(variable, invoke)
                        assign :> Expression, variable
                |]
                let expressions, variables = result |> Array.unzip
                let expressions = Array.append expressions [| Expression.Call(constructor, variables |> Seq.cast<Expression>) |]
                let block = Expression.Block(variables, expressions)
                block :> Expression

            match parameters with
            | null | [| |] -> Expression.Call(constructor) :> Expression
            | _ -> MakeBodyItem ()

        let switchCases =
            caseInfos
            |> Seq.map (fun x -> Expression.SwitchCase(MakeBody x, Expression.Constant(x.Tag)))
            |> Seq.toArray
        let getMethod = DecodeNumberMethodInfo
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Default(t))
        let block =
            Expression.Block(
                Seq.singleton flag,
                Expression.Assign(flag, Expression.Call(getMethod, span)),
                Expression.Switch(flag, defaultBlock, switchCases))
        let delegateType = typedefof<UnionDecoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, span, mark)
        lambda

    interface IConverterCreator with
        member __.GetConverter(context, t) =
            let Make (cases : UnionCaseInfo array) =
                let tagMember = FSharpValue.PreComputeUnionTagMemberInfo(t)
                let noNull = not t.IsValueType && not (tagMember :? MethodInfo)
                let encode = GetEncodeExpression context t cases tagMember false
                let decode = GetDecodeExpression context t cases false
                let encodeAuto = GetEncodeExpression context t cases tagMember true
                let decodeAuto = GetDecodeExpression context t cases true
                let converterType = typedefof<UnionConverter<_>>.MakeGenericType t
                let delegates = [| encode; decode; encodeAuto; decodeAuto |] |> Array.map (fun x -> x.Compile())
                let converterArguments = Array.append (delegates |> Seq.cast<obj> |> Seq.toArray) [| box noNull |]
                let converter = Activator.CreateInstance(converterType, converterArguments)
                converter :?> Converter

            if not (FSharpType.IsUnion(t)) || (t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<List<_>>) then
                null
            else
                let cases = FSharpType.GetUnionCases(t)
                if (cases = null || cases.Length = 0) then
                    raise (ArgumentException(sprintf "No available union case found, type: %O" t))
                let unionType = cases |> Array.map (fun x -> x.DeclaringType) |> Array.distinct |> Array.exactlyOne
                if (unionType <> t) then
                    raise (ArgumentException(sprintf "Invalid union type, you may have to use union type '%O' instead of case type '%O'" unionType t))
                Make cases
