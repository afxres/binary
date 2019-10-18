using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class UriAdapter : Adapter<Uri, string>
    {
        public override string Of(Uri item) => item?.OriginalString;

        public override Uri To(string item) => string.IsNullOrEmpty(item) ? null : new Uri(item);
    }
}
