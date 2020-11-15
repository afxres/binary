using System;
using System.Diagnostics;

namespace Mikodev.Binary.External
{
    internal sealed class Node<T>
    {
        public readonly long Header;

        public readonly Node<T>[] Values;

        public readonly bool Exists;

        public readonly T Intent;

        public Node(Node<T>[] values, long header, bool exists, T intent)
        {
            Debug.Assert(values is not null);
            this.Values = values;
            this.Header = header;
            this.Exists = exists;
            this.Intent = intent;
        }

        public override string ToString()
        {
            static string View(ulong source)
            {
                var span = (Span<char>)stackalloc char[sizeof(long)];
                for (var i = 0; i < sizeof(long); i++, source >>= 8)
                    span[i] = ((char)(byte)source is < ' ' or > '~') ? '.' : (char)(byte)source;
                return span.ToString();
            }

            var view = View((ulong)this.Header);
            var data = this.Exists ? $", {nameof(this.Intent)}: '{this.Intent}'" : string.Empty;
            var head = $"{nameof(Node<T>)}({nameof(this.Header)}: '{view}', {nameof(this.Values)}: {this.Values.Length}";
            return string.Concat(head, data, ")");
        }
    }
}
