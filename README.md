# Binary

Summary:
| Package                       | Version        | Downloads        | Descriptions        |
| :---------------------------- | :------------- | :--------------- | :------------------ |
| [`Mikodev.Binary`][PC]        | ![version][VC] | ![downloads][IC] | Main package        |
| [`Mikodev.Binary.FSharp`][PF] | ![version][VF] | ![downloads][IF] | Type support for F# |

Add package (for C# project):
```
dotnet add package Mikodev.Binary
```

Add package (for F# project):
```
dotnet add package Mikodev.Binary.FSharp
```

Sample code (F#):
```fsharp
open Mikodev.Binary

let generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build()
let source = {| text = "Hello, F#!"; list = [ 1; 0; 2; 4 ] |}
let buffer = generator.Encode source
let result = generator.Decode(buffer, anonymous = source)
printfn "%A" result
```

[PC]:https://www.nuget.org/packages/Mikodev.Binary/
[PF]:https://www.nuget.org/packages/Mikodev.Binary.FSharp/
[VC]:https://img.shields.io/nuget/v/Mikodev.Binary
[VF]:https://img.shields.io/nuget/v/Mikodev.Binary.FSharp
[IC]:https://img.shields.io/nuget/dt/Mikodev.Binary
[IF]:https://img.shields.io/nuget/dt/Mikodev.Binary.FSharp
