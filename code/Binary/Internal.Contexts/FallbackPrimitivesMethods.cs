namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal static class FallbackPrimitivesMethods
{
    private static readonly ImmutableArray<string> Names = ImmutableArray.Create(new[]
    {
        "Item1",
        "Item2",
        "Item3",
        "Item4",
        "Item5",
        "Item6",
        "Item7",
        "Rest",
    });

    private static readonly ImmutableArray<Type> Types = ImmutableArray.Create(new[]
    {
        typeof(Tuple<>),
        typeof(Tuple<,>),
        typeof(Tuple<,,>),
        typeof(Tuple<,,,>),
        typeof(Tuple<,,,,>),
        typeof(Tuple<,,,,,>),
        typeof(Tuple<,,,,,,>),
        typeof(Tuple<,,,,,,,>),
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
        typeof(ValueTuple<,,,,,,,>),
    });

    private static bool IsTupleOrValueTuple(Type type)
    {
        if (type == typeof(ValueTuple))
            throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
        return CommonModule.SelectGenericTypeDefinitionOrDefault(type, Types.Contains);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IConverter GetTupleConverter(IGeneratorContext context, Type type)
    {
        var names = Names.Take(type.GetGenericArguments().Length);
        var types = type.GetGenericArguments();
        var constructorInfo = CommonModule.GetPublicInstanceConstructor(type, types);
        var converters = types.Select(context.GetConverter).ToImmutableArray();
        var properties = names.Select(x => CommonModule.GetPublicInstanceProperty(type, x)).ToImmutableArray();
        var constructor = new ContextObjectConstructor((delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, constructorInfo));
        var initializers = properties.Select(x => new ContextMemberInitializer(e => Expression.Property(e, x))).ToImmutableArray();
        return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, initializers);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IConverter GetValueTupleConverter(IGeneratorContext context, Type type)
    {
        static void Invoke(Type type, Action<FieldInfo> action)
        {
            foreach (var i in Names.Take(type.GetGenericArguments().Length))
                action.Invoke(CommonModule.GetPublicInstanceField(type, i));
            return;
        }

        static void Expand(IGeneratorContext context, List<(Type, ContextMemberInitializer, IConverter)> result, FieldInfo field, ContextMemberInitializer parent)
        {
            var type = field.FieldType;
            var init = new ContextMemberInitializer(x => Expression.Field(parent.Invoke(x), field));
            var converter = context.GetConverter(type);
            if (type.IsValueType && IsTupleOrValueTuple(type) && converter.GetType() == typeof(TupleObjectConverter<>).MakeGenericType(type))
                Invoke(type, x => Expand(context, result, x, init));
            else
                result.Add((type, init, converter));
        }

        var result = new List<(Type Type, ContextMemberInitializer Member, IConverter Converter)>();
        Invoke(type, x => Expand(context, result, x, v => v));
        var members = result.Select(x => x.Member).ToImmutableArray();
        var converters = result.Select(x => x.Converter).ToImmutableArray();
        var constructor = new ContextObjectConstructor((delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, members));
        return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, members);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (IsTupleOrValueTuple(type) is false)
            return null;
        return type.IsValueType is false
            ? GetTupleConverter(context, type)
            : GetValueTupleConverter(context, type);
    }
}
