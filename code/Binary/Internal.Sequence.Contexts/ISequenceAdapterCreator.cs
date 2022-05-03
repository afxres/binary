namespace Mikodev.Binary.Internal.Sequence.Contexts;

using Mikodev.Binary.Internal.Sequence;

internal interface ISequenceAdapterCreator<T>
{
    SequenceAdapter<T> GetAdapter();
}
