# Mikodev.Binary

## Basic Usage

### C# Code
```csharp
using Mikodev.Binary;
using System;

namespace Sample
{
    class Program
    {
        static void Main()
        {
            var generator = new Generator();
            var source = new { number = 1024, text = "csharp", content = new { tuple = (1.2F, 3.4) } };
            var buffer = generator.ToBytes(source);
            var result = generator.ToValue(buffer, anonymous: source);
            Console.WriteLine(buffer.Length);
            Console.WriteLine(result);
        }
    }
}
```

### F# Code
```fsharp
open Mikodev.Binary

[<EntryPoint>]
let main _ =
    let generator = Generator()
    let source = {| number = 1024; text = "fsharp"; content = {| tuple = 1.2F, 3.4 |} |}
    let buffer = generator.ToBytes source
    let result = generator.ToValue(buffer, anonymous = source)
    printfn "%d" buffer.Length
    printfn "%A" result
    0
```
