namespace Mikodev.Binary.Creators;

using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal sealed class UnionConverterCreator : IConverterCreator
{
    private sealed record UnionCaseTypeWithCreateMethod(Type Type, MethodBase Create);

    private sealed record UnionCaseInfo(int Index, Type Type, MethodBase Create, IConverter Converter);

    private static readonly MethodInfo DecodeMethodInfo = new DecodeDelegate<int>(Converter.Decode).Method;

    private static readonly MethodInfo EncodeMethodInfo = new EncodeDelegate<int>(Converter.Encode).Method;

    private static readonly MethodInfo DecodeFailedMethodInfo = new Action<int>(ThrowHelper.ThrowInvalidUnionTag<object>).Method.GetGenericMethodDefinition();

    private static readonly MethodInfo EncodeFailedMethodInfo = new Action(ThrowHelper.ThrowInvalidOrNullUnionValue<object>).Method.GetGenericMethodDefinition();

    private static bool FilterUnionCreateMethod(MethodInfo m)
    {
        return m.Name is "Create" && m.IsStatic && m.GetParameters().Length is 1;
    }

    private static UnionCaseTypeWithCreateMethod? SelectUnionCaseTypeWithCreateMethod(MethodBase i)
    {
        var parameters = i.GetParameters();
        if (parameters.Length is not 1)
            return null;
        var parameter = parameters.Single();
        var unionCaseType = parameter switch
        {
            { IsIn: true, ParameterType.IsByRef: true } => parameter.ParameterType.GetElementType(),
            { ParameterType.IsByRef: false } => parameter.ParameterType,
            _ => null,
        };
        if (unionCaseType is null)
            return null;
        return new UnionCaseTypeWithCreateMethod(unionCaseType, i);
    }

    private static bool FilterUnionValueProperty(PropertyInfo p)
    {
        return p.Name is "Value" && p.PropertyType == typeof(object) && p.GetGetMethod(nonPublic: false) is not null && p.GetIndexParameters().Length is 0;
    }

    private static PropertyInfo SelectUnionValueProperty(Type type, ImmutableArray<Type?> types)
    {
        var property = types.OfType<Type>().SelectMany(x => x.GetProperties(CommonDefine.PublicInstanceBindingFlags)).FirstOrDefault(FilterUnionValueProperty);
        if (property is not null)
            return property;
        throw new ArgumentException($"Union value property detection failed, type: {type}");
    }

    private static Delegate GetEncodeDelegate(Type type, ImmutableArray<UnionCaseInfo> caseSet, PropertyInfo valueProperty, bool auto)
    {
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
        var union = Expression.Parameter(type, "item");
        var value = Expression.Property(union, valueProperty);
        var expressions = new List<Expression>();
        var target = Expression.Label("target");
        foreach (var i in caseSet)
        {
            var test = Expression.TypeIs(value, i.Type);
            var intent = Expression.Convert(value, i.Type);
            var ifTrue = Expression.Block(
                Expression.Call(EncodeMethodInfo, allocator, Expression.Constant(i.Index)),
                Expression.Call(Expression.Constant(i.Converter), Converter.GetMethod(i.Converter, auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode)), allocator, intent),
                Expression.Break(target));
            expressions.Add(Expression.IfThen(test, ifTrue));
        }
        if (type.IsValueType is false)
        {
            var tests = Expression.Block(expressions);
            expressions.Clear();
            expressions.Add(Expression.IfThen(Expression.NotEqual(union, Expression.Constant(null, type)), tests));
        }
        expressions.Add(Expression.Call(EncodeFailedMethodInfo.MakeGenericMethod(type)));
        expressions.Add(Expression.Label(target));
        var delegateType = typeof(AllocatorAction<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), [allocator, union]);
        return lambda.Compile();
    }

    private static Delegate GetDecodeDelegate(Type type, ImmutableArray<UnionCaseInfo> caseSet, bool auto)
    {
        var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
        var index = Expression.Variable(typeof(int), "index");
        var union = Expression.Variable(type, "union");
        var expressions = new List<Expression>();
        var cases = new List<SwitchCase>();
        expressions.Add(Expression.Assign(index, Expression.Call(DecodeMethodInfo, span)));
        foreach (var i in caseSet)
        {
            var decode = Expression.Call(Expression.Constant(i.Converter), Converter.GetMethod(i.Converter, auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode)), span);
            var create = i.Create is ConstructorInfo constructor
                ? Expression.New(constructor, decode)
                : Expression.Call((MethodInfo)i.Create, decode) as Expression;
            var assign = Expression.Assign(union, create);
            cases.Add(Expression.SwitchCase(Expression.Block(assign, Expression.Empty()), Expression.Constant(i.Index)));
        }
        expressions.Add(Expression.Switch(index, Expression.Call(DecodeFailedMethodInfo.MakeGenericMethod(type), index), [.. cases]));
        expressions.Add(union);
        var delegateType = typeof(DecodeDelegate<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block([index, union], expressions), [span]);
        return lambda.Compile();
    }

    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type.GetCustomAttributes(false).Any(x => x.GetType().FullName is "System.Runtime.CompilerServices.UnionAttribute") is false)
            return null;
        var systemUnionInterface = type.GetInterfaces().SingleOrDefault(x => x.FullName is "System.Runtime.CompilerServices.IUnion");
        var customUnionInterface = type.GetInterfaces().SingleOrDefault(x => x.Name is "IUnionMembers");
        var caseTypeList = default(List<UnionCaseTypeWithCreateMethod>);
        if (customUnionInterface is not null)
            caseTypeList = [.. customUnionInterface.GetMethods().Where(FilterUnionCreateMethod).Select(SelectUnionCaseTypeWithCreateMethod).OfType<UnionCaseTypeWithCreateMethod>()];
        if (caseTypeList is null or { Count: 0 })
            caseTypeList = [.. type.GetConstructors().Select(SelectUnionCaseTypeWithCreateMethod).OfType<UnionCaseTypeWithCreateMethod>()];
        if (caseTypeList is null or { Count: 0 } || caseTypeList.DistinctBy(x => x.Type).Count() != caseTypeList.Count)
            throw new ArgumentException($"Union case detection failed, type: {type}");
        var caseList = caseTypeList.Select((x, i) => new UnionCaseInfo(i, x.Type, x.Create, context.GetConverter(x.Type))).ToList();
        caseList.Sort((a, b) => CommonModule.CompareConversion(a.Type, b.Type));
        var cases = caseList.ToImmutableArray();
        var valueProperty = SelectUnionValueProperty(type, [type, systemUnionInterface, customUnionInterface]);
        var encode = GetEncodeDelegate(type, cases, valueProperty, auto: false);
        var encodeAuto = GetEncodeDelegate(type, cases, valueProperty, auto: true);
        var decode = GetDecodeDelegate(type, cases, auto: false);
        var decodeAuto = GetDecodeDelegate(type, cases, auto: true);
        var converter = CommonModule.CreateInstance(typeof(UnionConverter<>).MakeGenericType(type), [encode, encodeAuto, decode, decodeAuto]);
        return (IConverter)converter;
    }
}
