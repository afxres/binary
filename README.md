# Mikodev.Binary

## C#
### Add Package
```
dotnet add package Mikodev.Binary
```
### Sample Code
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
            var source = new { number = 1024, text = "csharp", content = new { tuple = (1.2F, 3.4) } };
            var buffer = generator.Encode(source);
            var result = generator.Decode(buffer, anonymous: source);
            Console.WriteLine(buffer.Length);
            Console.WriteLine(result);
        }
    }
}
```

## F#
### Add Package
```
dotnet add package Mikodev.Binary.FSharp
```
### Sample Code
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
