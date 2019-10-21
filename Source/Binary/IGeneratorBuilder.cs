namespace Mikodev.Binary
{
    public interface IGeneratorBuilder
    {
        IGeneratorBuilder AddConverter(Converter converter);

        IGeneratorBuilder AddConverterCreator(IConverterCreator creator);

        IGenerator Build();
    }
}
