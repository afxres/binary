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
    private sealed record UnionCaseInfo(int Index, Type Type, ConstructorInfo Constructor, IConverter Converter);

    private static readonly Comparer<UnionCaseInfo> UnionCaseInfoComparer = Comparer<UnionCaseInfo>.Create((x, y) => CompareInheritance(x.Type, y.Type));

    private static readonly MethodInfo DecodeMethodInfo = new DecodeDelegate<int>(Converter.Decode).Method;

    private static readonly MethodInfo EncodeMethodInfo = new EncodeDelegate<int>(Converter.Encode).Method;

    private static readonly MethodInfo DecodeFailedMethodInfo = new Action<int>(ThrowHelper.ThrowInvalidUnionTag<object>).Method.GetGenericMethodDefinition();

    private static int CompareInheritance(Type? x, Type? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        if (x.IsValueType || y.IsValueType)
            throw new ArgumentException("Require reference type.");
        if (x == y)
            throw new ArgumentException("Identical types detected.");
        if (x.IsAssignableTo(y))
            return -1;
        if (y.IsAssignableTo(x))
            return 1;
        return StringComparer.InvariantCulture.Compare(x.FullName, y.FullName);
    }

    private static Delegate GetEncodeDelegate(Type type, ImmutableSortedSet<UnionCaseInfo> unionCaseInfoSet, bool auto)
    {
        var property = type.GetProperties(CommonDefine.PublicInstanceBindingFlags).Single(x => x.Name is "Value" && x.PropertyType == typeof(object));
        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
        var union = Expression.Parameter(type, "item");
        var value = Expression.Property(union, property);
        var expressions = new List<Expression>();
        var target = Expression.Label("target");
        foreach (var i in unionCaseInfoSet)
        {
            var test = Expression.TypeIs(value, i.Type);
            var intent = Expression.Convert(value, i.Type);
            var ifTrue = Expression.Block(
                Expression.Call(EncodeMethodInfo, allocator, Expression.Constant(i.Index)),
                Expression.Call(Expression.Constant(i.Converter), Converter.GetMethod(i.Converter, auto ? nameof(IConverter.EncodeAuto) : nameof(IConverter.Encode)), allocator, intent),
                Expression.Break(target));
            expressions.Add(Expression.IfThen(test, ifTrue));
        }
        expressions.Add(Expression.Label(target));
        var delegateType = typeof(AllocatorAction<>).MakeGenericType(type);
        var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), [allocator, union]);
        return lambda.Compile();
    }

    private static Delegate GetDecodeDelegate(Type type, ImmutableSortedSet<UnionCaseInfo> unionCaseInfoSet, bool auto)
    {
        var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
        var index = Expression.Variable(typeof(int), "index");
        var union = Expression.Variable(type, "union");
        var expressions = new List<Expression>();
        var cases = new List<SwitchCase>();
        expressions.Add(Expression.Assign(index, Expression.Call(DecodeMethodInfo, span)));
        foreach (var i in unionCaseInfoSet)
        {
            var decode = Expression.Call(Expression.Constant(i.Converter), Converter.GetMethod(i.Converter, auto ? nameof(IConverter.DecodeAuto) : nameof(IConverter.Decode)), span);
            var create = Expression.New(i.Constructor, decode);
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
        var unionCaseInfoSetBuilder = ImmutableSortedSet.CreateBuilder(UnionCaseInfoComparer);
        foreach (var i in type.GetConstructors())
        {
            var parameters = i.GetParameters();
            if (parameters.Length is not 1)
                continue;
            var unionCaseType = parameters.Single().ParameterType;
            var unionCaseConverter = context.GetConverter(unionCaseType);
            _ = unionCaseInfoSetBuilder.Add(new UnionCaseInfo(unionCaseInfoSetBuilder.Count, unionCaseType, i, unionCaseConverter));
        }
        var unionCaseInfoSet = unionCaseInfoSetBuilder.ToImmutable();
        var encode = GetEncodeDelegate(type, unionCaseInfoSet, auto: false);
        var encodeAuto = GetEncodeDelegate(type, unionCaseInfoSet, auto: true);
        var decode = GetDecodeDelegate(type, unionCaseInfoSet, auto: false);
        var decodeAuto = GetDecodeDelegate(type, unionCaseInfoSet, auto: true);
        var converter = CommonModule.CreateInstance(typeof(UnionConverter<>).MakeGenericType(type), [encode, encodeAuto, decode, decodeAuto]);
        return (IConverter)converter;
    }
}
