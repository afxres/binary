namespace Mikodev.Binary.Creators.Tuples
{
    internal abstract class TupleLikeConverter<T> : Converter<T>
    {
        protected TupleLikeConverter(int length) : base(length) { }
    }
}
