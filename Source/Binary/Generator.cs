namespace Mikodev.Binary
{
    public static class Generator
    {
        public static IGenerator CreateDefault() => CreateDefaultBuilder().Build();

        public static IGeneratorBuilder CreateDefaultBuilder() => new GeneratorBuilder().AddDefaultConverterCreators();
    }
}
