using System;

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
            this.Values = values ?? throw new ArgumentNullException(nameof(values));
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
                    span[i] = ((byte)source < ' ' || (byte)source > '~') ? '.' : (char)(byte)source;
                return span.ToString();
            }

            var view = View((ulong)Header);
            var data = Exists ? $", {nameof(Intent)}: '{Intent}'" : string.Empty;
            var head = $"{nameof(Node<T>)}({nameof(Header)}: '{view}', {nameof(Values)}: {Values.Length}";
            return string.Concat(head, data, ")");
        }
    }
}
