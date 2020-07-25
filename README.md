# Binary
## Samples
### C#
```
dotnet add package Mikodev.Binary
```
```csharp
using Mikodev.Binary;
using System;

namespace Sample
{
    internal class Program
    {
        private static void Main()
        {
            var generator = Generator.CreateDefault();
            var source = new
            {
                number = 1024,
                text = "csharp",
                content = new { tuple = (1.2F, 3.4) }
            };
            var buffer = generator.Encode(source);
            var result = generator.Decode(buffer, anonymous: source);
            Console.WriteLine(buffer.Length);
            Console.WriteLine(result);
        }
    }
}
```
### F#
```
dotnet add package Mikodev.Binary.FSharp
```
```fsharp
open Mikodev.Binary

[<EntryPoint>]
let main _ =
    let generator =
        Generator.CreateDefaultBuilder()
            .AddFSharpConverterCreators()
            .Build()
    let source = {|
        id = 1024;
        text = "fsharp";
        tags = [ "F#"; ".NET" ];
        content = {| tuple = 1.2F, 3.4 |} |}
    let buffer = generator.Encode source
    let result = generator.Decode(buffer, anonymous = source)
    printfn "%d" buffer.Length
    printfn "%A" result
    0
```
## Implementations
### Enumerable
``` csharp
using Mikodev.Binary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sample
{
    internal class Program
    {
        private static readonly IGenerator generator = Generator.CreateDefault();

        private static void Main()
        {
            EnumerableTest(new[] { 1, 2, 3, 4 });
            EnumerableTest(new[] { "alpha", "bravo", "charlie" });
        }

        private static void EnumerableTest<E>(IEnumerable<E> data)
        {
            var a = data.ToArray();
            var b = data.ToList();
            var m = generator.Encode(a);
            var n = generator.Encode(b);
            var buffer = Encode(data);
            Console.WriteLine(MemoryExtensions.SequenceEqual<byte>(m, buffer)); // true
            Console.WriteLine(MemoryExtensions.SequenceEqual<byte>(n, buffer)); // true

            var x = Decode<E[], E>(buffer, x => x.ToArray());
            var y = Decode<List<E>, E>(buffer, x => x.ToList());
            Console.WriteLine(Enumerable.SequenceEqual(a, x)); // true
            Console.WriteLine(Enumerable.SequenceEqual(b, y)); // true
        }

        private static byte[] Encode<E>(IEnumerable<E> data)
        {
            if (data is null)
                return Array.Empty<byte>();
            var allocator = new Allocator();
            var converter = generator.GetConverter<E>();
            // call 'EncodeAuto' for all
            foreach (var i in data)
                converter.EncodeAuto(ref allocator, i);
            return allocator.AsSpan().ToArray();
        }

        private static T Decode<T, E>(byte[] buffer, Func<IEnumerable<E>, T> constructor)
        {
            var converter = generator.GetConverter<E>();
            var span = new ReadOnlySpan<byte>(buffer);
            var list = new List<E>();
            // call 'DecodeAuto' until span (byte stream) is empty
            while (!span.IsEmpty)
                list.Add(converter.DecodeAuto(ref span));
            // call runtime generated delegate
            return constructor.Invoke(list);
        }
    }
}
```