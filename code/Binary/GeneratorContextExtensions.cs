namespace Mikodev.Binary;

public static class GeneratorContextExtensions
{
    public static Converter<T> GetConverter<T>(this IGeneratorContext context) => (Converter<T>)context.GetConverter(typeof(T));

    public static Converter<T> GetConverter<T>(this IGeneratorContext context, T? anonymous) => context.GetConverter<T>();
}
