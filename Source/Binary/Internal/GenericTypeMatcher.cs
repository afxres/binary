using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal
{
    internal readonly struct GenericTypeMatcher
    {
        private readonly IReadOnlyDictionary<Type, Type> dictionary;

        public GenericTypeMatcher(IReadOnlyDictionary<Type, Type> dictionary)
        {
            Debug.Assert(dictionary.Any());
            Debug.Assert(dictionary.All(x => x.Key.GetGenericArguments().Length == x.Value.GetGenericArguments().Length));
            this.dictionary = dictionary;
        }

        public bool Match(Type type, out Type converterDefinition)
        {
            Debug.Assert(type != null);
            Debug.Assert(dictionary != null);
            if (!type.IsGenericType)
                goto fail;
            var definition = type.GetGenericTypeDefinition();
            return dictionary.TryGetValue(definition, out converterDefinition);
        fail:
            converterDefinition = null;
            return false;
        }
    }
}
