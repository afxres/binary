using Mikodev.Binary.Converters.Runtime;
using Mikodev.Binary.Delegates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using ItemInitializer = System.Func<(System.Linq.Expressions.ParameterExpression, System.Linq.Expressions.Expression[])>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal
{
    internal sealed partial class GeneratorContext
    {
        private Converter GetConverterAsTupleObject(Type type, ConstructorInfo constructor, ItemIndexes indexes, MetaList metadata)
        {
            var toBytes = ToBytesAsTupleObject(type, metadata, withMark: false);
            var toValue = ToValueAsTupleObject(type, metadata, constructor, indexes, withMark: false);
            var toBytesWith = ToBytesAsTupleObject(type, metadata, withMark: true);
            var toValueWith = ToValueAsTupleObject(type, metadata, constructor, indexes, withMark: true);
            var converterLength = Define.GetConverterLength(metadata.Select(x => x.Converter).ToArray());
            return (Converter)Activator.CreateInstance(typeof(TupleObjectConverter<>).MakeGenericType(type), toBytes, toValue, toBytesWith, toValueWith, converterLength);
        }

        private Converter GetConverterAsNamedObject(Type type, ConstructorInfo constructor, ItemIndexes indexes, MetaList metadata, NameDictionary dictionary)
        {
            if (dictionary == null)
                dictionary = metadata.Select(x => x.Property).ToDictionary(x => x, x => x.Name);
            var toBytes = ToBytesAsNamedObject(type, metadata, dictionary);
            var toValue = ToValueAsNamedObject(type, metadata, constructor, indexes);
            var buffers = metadata.Select(x => dictionary[x.Property]).Select(x => new KeyValuePair<string, byte[]>(x, GetOrCache(x))).ToArray();
            var properties = metadata.Select(x => x.Property).ToArray();
            return (Converter)Activator.CreateInstance(typeof(NamedObjectConverter<>).MakeGenericType(type), toBytes, toValue, properties, buffers);
        }

        #region constructor
        private bool CanCreateItem(Type type, ConstructorInfo constructor)
        {
            if (type.IsAbstract || type.IsInterface)
                return false;
            Debug.Assert(constructor == null || constructor.DeclaringType == type);
            // create instance by constructor or default value (for value type)
            return constructor != null || type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null;
        }

        private (ConstructorInfo Constructor, IReadOnlyList<PropertyInfo>) CanCreateWith(Dictionary<string, PropertyInfo> properties, ConstructorInfo constructor)
        {
            int parameterCount;
            var parameters = constructor.GetParameters();
            if (parameters == null || (parameterCount = parameters.Length) != properties.Count)
                return default;
            var collection = new PropertyInfo[parameterCount];
            for (var i = 0; i < parameterCount; i++)
            {
                var parameter = parameters[i];
                var parameterName = parameter.Name.ToUpperInvariant();
                if (!properties.TryGetValue(parameterName, out var property) || property.PropertyType != parameter.ParameterType)
                    return default;
                collection[i] = property;
            }
            Debug.Assert(collection.All(x => x != null));
            return (constructor, collection);
        }

        private (ConstructorInfo, ItemIndexes) GetConstructor(Type type, IReadOnlyList<PropertyInfo> properties)
        {
            // anonymous type or record
            var constructors = type.GetConstructors();
            if (constructors == null || constructors.Length == 0)
                return default;
            var names = properties.ToDictionary(x => x.Name.ToUpperInvariant());
            var query = constructors.Select(x => CanCreateWith(names, x)).Where(x => x.Constructor != null).ToList();
            if (query.Count == 0)
                return default;
            if (query.Count != 1)
                throw new ArgumentException($"Multiple constructors found, type: {type}");
            var (constructor, collection) = query.Single();
            var value = properties.Select((x, i) => (Key: x, Value: i)).ToDictionary(x => x.Key, x => x.Value);
            Debug.Assert(properties.Count == collection.Count);
            var array = collection.Select(x => value[x]).ToArray();
            return (constructor, array);
        }
        #endregion

        #region to bytes as named object or tuple object
        private Delegate ToBytesAsNamedObject(Type type, MetaList metadata, NameDictionary dictionary)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var buffer = GetOrCache(dictionary[property]);
                var propertyType = property.PropertyType;
                var propertyExpression = Expression.Property(item, property);
                var methodInfo = typeof(Converter<>).MakeGenericType(propertyType).GetMethod(nameof(IConverter.ToBytesWithLengthPrefix));
                expressions.Add(Expression.Call(allocator, appendWithLengthPrefixMethodInfo, Expression.Constant(buffer)));
                expressions.Add(Expression.Call(Expression.Constant(converter), methodInfo, allocator, propertyExpression));
            }
            var delegateType = typeof(OfNamedObject<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }

        private Delegate ToBytesAsTupleObject(Type type, MetaList metadata, bool withMark)
        {
            var item = Expression.Parameter(type, "item");
            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var expressions = new List<Expression>();

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var propertyExpression = Expression.Property(item, property);
                var method = Define.GetToBytesMethodInfo(property.PropertyType, withMark || i != metadata.Count - 1);
                expressions.Add(Expression.Call(Expression.Constant(converter), method, allocator, propertyExpression));
            }
            var delegateType = typeof(ToBytesWith<>).MakeGenericType(type);
            var lambda = Expression.Lambda(delegateType, Expression.Block(expressions), allocator, item);
            return lambda.Compile();
        }
        #endregion

        #region to value as named object or tuple object
        private (ParameterExpression, Expression[]) InitializeAsNamedObject(MetaList metadata)
        {
            var list = Expression.Parameter(typeof(LengthList).MakeByRefType(), "list");
            var values = new Expression[metadata.Count];

            for (var i = 0; i < metadata.Count; i++)
            {
                var (_, converter) = metadata[i];
                var method = invokeMethodInfo.MakeGenericMethod(converter.ItemType);
                var invoke = Expression.Call(list, method, Expression.Constant(converter), Expression.Constant(i));
                values[i] = invoke;
            }
            return (list, values);
        }

        private (ParameterExpression, Expression[]) InitializeAsTupleObject(MetaList metadata, bool withMark)
        {
            var span = Expression.Parameter(typeof(ReadOnlySpan<byte>).MakeByRefType(), "span");
            var values = new Expression[metadata.Count];

            for (var i = 0; i < metadata.Count; i++)
            {
                var (property, converter) = metadata[i];
                var method = Define.GetToValueMethodInfo(property.PropertyType, withMark || i != metadata.Count - 1);
                var invoke = Expression.Call(Expression.Constant(converter), method, span);
                values[i] = invoke;
            }
            return (span, values);
        }

        private Delegate ToValueAsNamedObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes)
        {
            if (!CanCreateItem(type, constructor))
                return null;
            var delegateType = typeof(ToNamedObject<>).MakeGenericType(type);
            return constructor == null
                ? ToValuePlanAlpha(delegateType, () => InitializeAsNamedObject(metadata), metadata, type)
                : ToValuePlanBravo(delegateType, () => InitializeAsNamedObject(metadata), metadata, indexes, constructor);
        }

        private Delegate ToValueAsTupleObject(Type type, MetaList metadata, ConstructorInfo constructor, ItemIndexes indexes, bool withMark)
        {
            if (!CanCreateItem(type, constructor))
                return null;
            var delegateType = typeof(ToValueWith<>).MakeGenericType(type);
            return constructor == null
                ? ToValuePlanAlpha(delegateType, () => InitializeAsTupleObject(metadata, withMark), metadata, type)
                : ToValuePlanBravo(delegateType, () => InitializeAsTupleObject(metadata, withMark), metadata, indexes, constructor);
        }

        private Delegate ToValuePlanAlpha(Type delegateType, ItemInitializer initializer, MetaList metadata, Type type)
        {
            var failed = metadata.Select(x => x.Property).FirstOrDefault(x => x.GetSetMethod() == null);
            if (failed != null)
                throw new ArgumentException($"Property '{failed.Name}' does not have a public setter, type: {type}");
            var item = Expression.Variable(type, "item");
            var expressions = new List<Expression> { Expression.Assign(item, Expression.New(type)) };
            var targets = metadata.Select(x => Expression.Property(item, x.Property)).ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < metadata.Count; i++)
                expressions.Add(Expression.Assign(targets[i], values[i]));
            expressions.Add(item);
            var lambda = Expression.Lambda(delegateType, Expression.Block(new[] { item }, expressions), parameter);
            return lambda.Compile();
        }

        private Delegate ToValuePlanBravo(Type delegateType, ItemInitializer initializer, MetaList metadata, ItemIndexes indexes, ConstructorInfo constructor)
        {
            var expressions = new List<Expression>();
            var variables = metadata.Select((x, i) => Expression.Variable(x.Property.PropertyType, $"{i}")).ToList();
            var (parameter, values) = initializer.Invoke();
            for (var i = 0; i < metadata.Count; i++)
                expressions.Add(Expression.Assign(variables[i], values[i]));
            expressions.Add(Expression.New(constructor, indexes.Select(x => variables[x]).ToList()));
            var lambda = Expression.Lambda(delegateType, Expression.Block(variables, expressions), parameter);
            return lambda.Compile();
        }
        #endregion
    }
}
