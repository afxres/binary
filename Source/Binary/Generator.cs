using Mikodev.Binary.Internal.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary
{
    public static class Generator
    {
        private static readonly IReadOnlyCollection<IConverterCreator> Creators = typeof(Converter).Assembly.GetTypes()
            .Where(x => !x.IsAbstract && typeof(IConverterCreator).IsAssignableFrom(x))
            .OrderBy(x => x.Namespace)
            .ThenBy(x => x.Name)
            .Select(x => (IConverterCreator)Activator.CreateInstance(x))
            .ToList();

        public static IGenerator CreateDefault()
        {
            return CreateDefaultBuilder().Build();
        }

        public static IGeneratorBuilder CreateDefaultBuilder()
        {
            var builder = new GeneratorBuilder();
            foreach (var creator in Creators)
                _ = builder.AddConverterCreator(creator);
            return builder;
        }
    }
}
