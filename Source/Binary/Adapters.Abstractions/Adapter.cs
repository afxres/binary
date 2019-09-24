using Mikodev.Binary.Adapters.Unsafe;
using Mikodev.Binary.Delegates;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Adapters.Abstractions
{
    internal abstract class Adapter
    {
        private static readonly bool available;

        private static readonly string arrayName;

        private static readonly string countName;

        static Adapter()
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

        protected static void CreateDelegates<T>(ref GetListItems<T> getList, ref SetListItems<T> setList)
        {
            if (!available)
                return;

            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var arrayField = typeof(List<T>).GetField(arrayName, flags);
            var countField = typeof(List<T>).GetField(countName, flags);
            var value = Expression.Parameter(typeof(List<T>), "value");
            var get = Expression.Lambda<GetListItems<T>>(Expression.Field(value, arrayField), value);

            var array = Expression.Parameter(typeof(T[]), "array");
            var block = Expression.Block(
                Expression.Assign(Expression.Field(value, arrayField), array),
                Expression.Assign(Expression.Field(value, countField), Expression.ArrayLength(array)));
            var set = Expression.Lambda<SetListItems<T>>(block, value, array);

            getList = get.Compile();
            setList = set.Compile();
        }

        public static Adapter Create(Converter converter)
        {
            if (converter.IsUnsafePrimitiveConverter())
                return (Adapter)Activator.CreateInstance(typeof(UnsafePrimitiveAdapter<>).MakeGenericType(converter.ItemType));
            var adapterType = converter.Length > 0 ? typeof(ConstantAdapter<>) : typeof(VariableAdapter<>);
            return (Adapter)Activator.CreateInstance(adapterType.MakeGenericType(converter.ItemType), converter);
        }
    }
}
