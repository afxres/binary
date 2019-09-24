using Mikodev.Binary.Adapters.Abstractions;
using Mikodev.Binary.Adapters.Implementations;
using Mikodev.Binary.Adapters.Implementations.Unsafe;
using Mikodev.Binary.Delegates;
using Mikodev.Binary.Internal;
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

            var value = typeof(List<int>).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var array = value.Where(x => x.FieldType == typeof(int[])).ToList();
            var count = value.Where(x => x.FieldType == typeof(int) && Validate(x)).ToList();
            available = array.Count == 1 && count.Count == 1;
            Debug.Assert(available);
            arrayName = available ? array.Single().Name : null;
            countName = available ? count.Single().Name : null;
        }

        private static void CreateDelegates<T>(out GetListItems<T> getList, out SetListItems<T> setList)
        {
            if (!available)
            {
                getList = null;
                setList = null;
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var arrayField = typeof(List<T>).GetField(arrayName, flags);
            var countField = typeof(List<T>).GetField(countName, flags);
            var value = Expression.Parameter(typeof(List<T>), "value");
            var get = Expression.Lambda<GetListItems<T>>(Expression.Field(value, arrayField), value);

            var array = Expression.Parameter(typeof(T[]), "array");
            var count = Expression.Parameter(typeof(int), "count");
            var block = Expression.Block(
                Expression.Assign(Expression.Field(value, arrayField), array),
                Expression.Assign(Expression.Field(value, countField), count));
            var set = Expression.Lambda<SetListItems<T>>(block, value, array, count);

            getList = get.Compile();
            setList = set.Compile();
        }

        private static Adapter<T> CreateInstance<T>(Converter<T> converter)
        {
            var adapter = converter.IsUnsafePrimitiveConverter()
                ? Activator.CreateInstance(typeof(UnsafePrimitiveAdapter<>).MakeGenericType(converter.ItemType))
                : Activator.CreateInstance((converter.Length > 0 ? typeof(ConstantAdapter<>) : typeof(VariableAdapter<>)).MakeGenericType(converter.ItemType), converter);
            CreateDelegates<T>(out var get, out var set);
            return new Adapter<T>((AdapterMember<T>)adapter, get, set);
        }

        public static object Create(Converter converter)
        {
            var method = typeof(AdapterHelper).GetMethod(nameof(CreateInstance), BindingFlags.Static | BindingFlags.NonPublic);
            var target = method.MakeGenericMethod(converter.ItemType);
            return target.Invoke(null, new object[] { converter });
        }
    }
}
