using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary
{
    public static class GeneratorBuilderExtensions
    {
        private static readonly IReadOnlyCollection<IConverterCreator> creators = typeof(Converter).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(IConverterCreator).IsAssignableFrom(x))
            .OrderBy(x => x.Namespace)
            .ThenBy(x => x.Name)
            .Select(x => (IConverterCreator)Activator.CreateInstance(x))
            .ToList();

        public static IGeneratorBuilder AddDefaultConverterCreators(this IGeneratorBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            foreach (var creator in creators)
                _ = builder.AddConverterCreator(creator);
            return builder;
        }
    }
}
