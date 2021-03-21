using Mikodev.Binary.Internal.Contexts.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackPrimitivesMethods
    {
        private static readonly IReadOnlyCollection<string> Names = new[]
        {
            "Item1",
            "Item2",
            "Item3",
            "Item4",
            "Item5",
            "Item6",
            "Item7",
            "Rest",
        };

        private static readonly IReadOnlyCollection<Type> Types = new[]
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
        };

        private static bool IsTupleOrValueTuple(Type type)
        {
            if (type == typeof(ValueTuple))
                throw new ArgumentException($"Invalid type: {typeof(ValueTuple)}");
            return CommonHelper.SelectGenericTypeDefinitionOrDefault(type, Types.Contains);
        }

        private static IConverter GetTupleConverter(IGeneratorContext context, Type type)
        {
            var names = Names.Take(type.GetGenericArguments().Length);
            var types = type.GetGenericArguments();
            var constructorInfo = CommonHelper.GetConstructor(type, types);
            var converters = types.Select(context.GetConverter).ToList();
            var properties = names.Select(x => CommonHelper.GetProperty(type, x, BindingFlags.Instance | BindingFlags.Public)).ToList();
            var constructor = new ContextObjectConstructor((delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, constructorInfo));
            return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, types, ContextMethods.GetMemberInitializers(properties));
        }

        private static IConverter GetValueTupleConverter(IGeneratorContext context, Type type)
        {
            static void Fields(Type type, Action<FieldInfo> action)
            {
                var names = Names.Take(type.GetGenericArguments().Length);
                var fields = names.Select(x => CommonHelper.GetField(type, x, BindingFlags.Instance | BindingFlags.Public)).ToList();
                fields.ForEach(action);
            }

            static void Expand(IGeneratorContext context, List<(Type, ContextMemberInitializer, IConverter)> result, FieldInfo field, ContextMemberInitializer parent)
            {
                var type = field.FieldType;
                var func = new ContextMemberInitializer(x => Expression.Field(parent.Invoke(x), field));
                var converter = context.GetConverter(type);
                if (type.IsValueType && IsTupleOrValueTuple(type) && converter.GetType() == typeof(TupleObjectConverter<>).MakeGenericType(type))
                    Fields(type, x => Expand(context, result, x, func));
                else
                    result.Add((type, x => func.Invoke(x), converter));
            }

            var result = new List<(Type Type, ContextMemberInitializer Member, IConverter Converter)>();
            Fields(type, x => Expand(context, result, x, v => v));
            var types = result.Select(x => x.Type).ToList();
            var members = result.Select(x => x.Member).ToList();
            var converters = result.Select(x => x.Converter).ToList();
            var constructor = new ContextObjectConstructor((delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, members));
            return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, types, members);
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (IsTupleOrValueTuple(type) is false)
                return null;
            return type.IsValueType is false
                ? GetTupleConverter(context, type)
                : GetValueTupleConverter(context, type);
        }
    }
}
