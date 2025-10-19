namespace Mikodev.Binary.Creators

open Microsoft.FSharp.Reflection
open Mikodev.Binary
open Mikodev.Binary.Internal
open System
open System.Linq.Expressions
open System.Reflection

type internal UnionTypeRecord = { Type: Type; GetConverter: Type -> IConverter; HasSelfTypeReference: bool }

type internal UnionConverterCreator() =
    static let EnsureSufficientExecutionStackExpression: Expression = Expression.Call(ModuleHelper.EnsureSufficientExecutionStackMethodInfo)

    static let GetEncodeExpression (typeRecord: UnionTypeRecord) (caseInfos: Map<int, UnionCaseInfo>) (tagMember: MemberInfo) (auto: bool) =
        let t = typeRecord.Type
        let getConverter = typeRecord.GetConverter
        let allocator = Expression.Parameter(ModuleHelper.AllocatorByRefType, "allocator")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let item = Expression.Parameter(t, "item")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (instance: Expression) (properties: PropertyInfo array) : Expression array = [|
            for i = 0 to properties.Length - 1 do
                let property = properties[i]
                let propertyType = property.PropertyType
                let converter = getConverter propertyType
                let method = Converter.GetMethod(converter, if auto || i <> properties.Length - 1 then "EncodeAuto" else "Encode")
                let invoke = Expression.Call(Expression.Constant(converter), method, allocator, Expression.Property(instance, property))
                invoke
        |]

        let MakeCase (info: UnionCaseInfo) : Expression =
            let properties = info.GetFields()

            let MakeCastCase (dataType: Type) =
                let data = Expression.Variable(dataType, "data")
                let cast = Expression.Assign(data, Expression.Convert(item, dataType))
                let body = MakeBody data properties
                Expression.Block(Seq.singleton data, Array.append<Expression> [| cast |] body)

            if properties |> Array.isEmpty then
                Expression.Empty() :> Expression
            else
                let dataType = properties |> Seq.map (fun x -> x.DeclaringType |> nonNull) |> Seq.distinct |> Seq.exactlyOne
                if t = dataType then
                    Expression.Block(MakeBody item properties) :> Expression
                else
                    MakeCastCase dataType :> Expression

        let switchCases = caseInfos |> Seq.map (fun x -> Expression.SwitchCase(MakeCase x.Value, Expression.Constant(x.Key))) |> Seq.toArray
        let tagExpression =
            match tagMember with
            | :? PropertyInfo as p -> Expression.Property(item, p) :> Expression
            | _ -> Expression.Call(tagMember :?> MethodInfo, item) :> Expression
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Empty())
        let basicExpressions: Expression array = [|
            Expression.Assign(flag, tagExpression)
            Expression.Call(ModuleHelper.EncodeNumberMethodInfo, allocator, flag)
            Expression.Switch(flag, defaultBlock, switchCases)
        |]
        let blockExpressions =
            if typeRecord.HasSelfTypeReference then
                Array.append [| EnsureSufficientExecutionStackExpression |] basicExpressions
            else
                basicExpressions
        let block = Expression.Block(Seq.singleton flag, blockExpressions)
        let delegateType = typedefof<UnionEncoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, allocator, item, mark)
        lambda

    static let GetDecodeExpression (typeRecord: UnionTypeRecord) (constructorInfos: Map<int, MethodInfo>) (auto: bool) =
        let t = typeRecord.Type
        let getConverter = typeRecord.GetConverter
        let span = Expression.Parameter(ModuleHelper.ReadOnlySpanByteByRefType, "span")
        let mark = Expression.Parameter(typeof<int>.MakeByRefType(), "mark")
        let flag = Expression.Variable(typeof<int>, "flag")

        let MakeBody (constructor: MethodInfo) : Expression =
            let parameters = constructor.GetParameters()

            let MakeBodyItem () : Expression =
                let result = [|
                    for i = 0 to parameters.Length - 1 do
                        let parameterType = parameters[i].ParameterType
                        let converter = getConverter parameterType
                        let method = Converter.GetMethod(converter, if auto || i <> parameters.Length - 1 then "DecodeAuto" else "Decode")
                        let variable = Expression.Variable(parameterType, string i)
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
                MakeBodyItem()

        let switchCases = constructorInfos |> Seq.map (fun x -> Expression.SwitchCase(MakeBody x.Value, Expression.Constant(x.Key))) |> Seq.toArray
        let defaultBlock = Expression.Block(Expression.Assign(mark, flag), Expression.Default(t))
        let basicExpressions: Expression array = [|
            Expression.Assign(flag, Expression.Call(ModuleHelper.DecodeNumberMethodInfo, span))
            Expression.Switch(flag, defaultBlock, switchCases)
        |]
        let blockExpressions =
            if typeRecord.HasSelfTypeReference then
                Array.append [| EnsureSufficientExecutionStackExpression |] basicExpressions
            else
                basicExpressions
        let block = Expression.Block(Seq.singleton flag, blockExpressions)
        let delegateType = typedefof<UnionDecoder<_>>.MakeGenericType t
        let lambda = Expression.Lambda(delegateType, block, span, mark)
        lambda

    interface IConverterCreator with
        member __.GetConverter(context, t) =
            let Make (cases: UnionCaseInfo array) =
                let caseInfos = cases |> Seq.map (fun x -> x.Tag, x) |> Map.ofSeq
                let constructorInfos = cases |> Seq.map (fun x -> x.Tag, FSharpValue.PreComputeUnionConstructorInfo(x)) |> Map.ofSeq
                let memberTypes = seq {
                    for i in caseInfos do
                        for f in i.Value.GetFields() -> f.PropertyType
                    for i in constructorInfos do
                        for p in i.Value.GetParameters() -> p.ParameterType
                }
                let converterType = typedefof<UnionConverter<_>>.MakeGenericType t
                let converter = Activator.CreateInstance(converterType, null) |> nonNull :?> IConverter
                let converters =
                    memberTypes |> Seq.distinct |> Seq.map (fun x -> x, if x = t then converter else CommonHelper.GetConverter(context, x)) |> readOnlyDict
                let getConverter = fun x -> converters[x]
                let tagMember = FSharpValue.PreComputeUnionTagMemberInfo(t)
                let needNullCheck = t.IsValueType = false && tagMember :? MethodInfo = false
                let typeRecord = { Type = t; GetConverter = getConverter; HasSelfTypeReference = memberTypes |> Seq.contains t }
                let encode = GetEncodeExpression typeRecord caseInfos tagMember false
                let encodeAuto = GetEncodeExpression typeRecord caseInfos tagMember true
                let decode = GetDecodeExpression typeRecord constructorInfos false
                let decodeAuto = GetDecodeExpression typeRecord constructorInfos true
                let delegates = [| encode; encodeAuto; decode; decodeAuto |] |> Array.map (fun x -> x.Compile())
                let initializeArguments = Array.append (delegates |> Array.map box) [| box needNullCheck |]
                let initializeMethod =
                    converterType.GetMethods(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic)
                    |> Array.filter (fun x -> x.Name = "Initialize")
                    |> Array.exactlyOne
                initializeMethod.Invoke(converter, initializeArguments) |> ignore
                converter

            if FSharpType.IsUnion(t) = false || (t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<_ list>) then
                null
            else
                let cases = FSharpType.GetUnionCases(t)
                if (cases |> Array.isEmpty) then
                    raise (ArgumentException $"No available union case found, type: {t}")
                let unionType = cases |> Seq.map (fun x -> x.DeclaringType) |> Seq.distinct |> Seq.exactlyOne
                if (unionType <> t) then
                    raise (ArgumentException $"Invalid union type, you may have to use union type '{unionType}' instead of case type '{t}'")
                Make cases
