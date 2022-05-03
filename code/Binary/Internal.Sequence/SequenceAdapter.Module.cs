namespace Mikodev.Binary.Internal.Sequence;

using Mikodev.Binary.Internal.Sequence.Adapters;
using Mikodev.Binary.Internal.Sequence.Contexts;

internal static class SequenceAdapter
{
    internal static SequenceAdapter<T> Create<T>(Converter<T> converter)
    {
        if (converter is ISequenceAdapterCreator<T> creator)
            return creator.GetAdapter();
        if (converter.Length > 0)
            return new ConstantAdapter<T>(converter);
        else
            return new VariableAdapter<T>(converter);
    }
}
