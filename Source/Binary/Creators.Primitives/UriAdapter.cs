using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class UriAdapter : Adapter<Uri, string>
    {
        public override string OfValue(Uri item) => item?.OriginalString;

        public override Uri ToValue(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);
    }
}
