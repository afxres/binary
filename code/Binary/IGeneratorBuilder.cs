namespace Mikodev.Binary
{
    public interface IGeneratorBuilder
    {
        IGeneratorBuilder AddConverter(IConverter converter);

        IGeneratorBuilder AddConverterCreator(IConverterCreator creator);

        IGenerator Build();
    }
}
