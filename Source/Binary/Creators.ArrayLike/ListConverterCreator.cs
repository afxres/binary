using Mikodev.Binary.Internal.Delegates;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Creators.ArrayLike
{
    internal sealed class ListConverterCreator : IConverterCreator
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly bool available;

        private static readonly string arrayName;

        private static readonly string countName;

        static ListConverterCreator()
        {
            static bool Validate(FieldInfo field)
            {
                var times = 4;
                var match = 0;
                var items = new List<int>(times);
                for (var i = 0; i < times; i++)
                {
                    field.SetValue(items, i);
                    match += (items.Count == i) ? 1 : 0;
                }
                return match == times;
            }

            var value = typeof(List<int>).GetFields(FieldFlags);
            var array = value.Where(x => x.FieldType == typeof(int[])).ToList();
            var count = value.Where(x => x.FieldType == typeof(int) && Validate(x)).ToList();
            available = array.Count == 1 && count.Count == 1 && typeof(List<>).GetConstructor(Type.EmptyTypes) != null;
            Debug.Assert(available);
            arrayName = available ? array.Single().Name : null;
            countName = available ? count.Single().Name : null;
        }

        private static object[] CreateDelegates(Type type)
        {
            Debug.Assert(available);

            var listType = typeof(List<>).MakeGenericType(type);
            var arrayField = listType.GetField(arrayName, FieldFlags);
            var countField = listType.GetField(countName, FieldFlags);

            LambdaExpression Of()
            {
                var value = Expression.Parameter(listType, "value");
                var field = Expression.Field(value, arrayField);
                return Expression.Lambda(typeof(OfList<>).MakeGenericType(type), field, value);
            }

            LambdaExpression To()
            {
                var array = Expression.Parameter(type.MakeArrayType(), "array");
                var count = Expression.Parameter(typeof(int), "count");
                var value = Expression.Variable(listType, "value");
                var block = Expression.Block(
                    new[] { value },
                    Expression.Assign(value, Expression.New(listType)),
                    Expression.Assign(Expression.Field(value, arrayField), array),
                    Expression.Assign(Expression.Field(value, countField), count),
                    value);
                return Expression.Lambda(typeof(ToList<>).MakeGenericType(type), block, array, count);
            }

            var of = Of();
            var to = To();
            return new object[] { of.Compile(), to.Compile() };
        }

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.TryGetGenericArguments(typeof(List<>), out var types))
                return null;
            var itemType = types.Single();
            var itemConverter = context.GetConverter(itemType);
            var builderDefinition = available
                ? typeof(ListDelegateBuilder<>)
                : typeof(ListFallbackBuilder<>);
            var builderArguments = available ? CreateDelegates(itemType) : Array.Empty<object>();
            var builderType = builderDefinition.MakeGenericType(itemType);
            var builder = Activator.CreateInstance(builderType, builderArguments);
            var converterArguments = new object[] { itemConverter, builder };
            var converterType = typeof(ArrayLikeConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
