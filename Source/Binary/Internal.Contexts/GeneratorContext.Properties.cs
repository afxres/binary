using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class GeneratorContext
    {
        private (ConstructorInfo Constructor, IReadOnlyList<PropertyInfo>) CanCreateInstanceWith(ConstructorInfo constructor, Dictionary<string, PropertyInfo> properties)
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
            var query = constructors.Select(x => CanCreateInstanceWith(x, names)).Where(x => x.Constructor != null).ToList();
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
    }
}
