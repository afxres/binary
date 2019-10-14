using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Adapters.Implementations;
using Mikodev.Binary.Adapters.Implementations.Unsafe;
using Mikodev.Binary.Internal.Delegates;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Adapters
{
    internal static class AdapterHelper
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly bool available;

        private static readonly string arrayName;

        private static readonly string countName;

        static AdapterHelper()
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

        private static void CreateDelegates<T>(out OfList<T> ofList, out ToList<T> toList)
        {
            if (!available)
            {
                ofList = null;
                toList = null;
                return;
            }

            var arrayField = typeof(List<T>).GetField(arrayName, FieldFlags);
            var countField = typeof(List<T>).GetField(countName, FieldFlags);

            Expression<OfList<T>> Of()
            {
                var value = Expression.Parameter(typeof(List<T>), "value");
                var field = Expression.Field(value, arrayField);
                return Expression.Lambda<OfList<T>>(field, value);
            }

            Expression<ToList<T>> To()
            {
                var array = Expression.Parameter(typeof(T[]), "array");
                var count = Expression.Parameter(typeof(int), "count");
                var value = Expression.Variable(typeof(List<T>), "value");
                var block = Expression.Block(
                    new[] { value },
                    Expression.Assign(value, Expression.New(typeof(List<T>))),
                    Expression.Assign(Expression.Field(value, arrayField), array),
                    Expression.Assign(Expression.Field(value, countField), count),
                    value);
                return Expression.Lambda<ToList<T>>(block, array, count);
            }

            var of = Of();
            var to = To();
            ofList = of.Compile();
            toList = to.Compile();
        }

        internal static Adapter<T> Create<T>(Converter<T> converter)
        {
            var adapter = converter.IsUnsafeNativeConverter()
                ? Activator.CreateInstance(typeof(UnsafeNativeAdapter<>).MakeGenericType(converter.ItemType))
                : Activator.CreateInstance((converter.Length > 0 ? typeof(ConstantAdapter<>) : typeof(VariableAdapter<>)).MakeGenericType(converter.ItemType), converter);
            CreateDelegates<T>(out var get, out var set);
            return new Adapter<T>((AdapterMember<T>)adapter, get, set);
        }
    }
}
