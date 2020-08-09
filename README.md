# Binary

Summary:
| Package                       | Version        | Downloads        | Descriptions |
| :---------------------------- | :------------- | :--------------- | :----------- |
| [`Mikodev.Binary`][PC]        | ![version][VC] | ![downloads][IC] | Main package |
| [`Mikodev.Binary.FSharp`][PF] | ![version][VF] | ![downloads][IF] | Additional converters for F# |

Install package:
```
dotnet add package Mikodev.Binary
```

Import namespace:
```csharp
using Mikodev.Binary;
```

Sample code:
```csharp
var source = new { text = "Hello, world!" };
var generator = Generator.CreateDefault();
var buffer = generator.Encode(source);
var result = generator.Decode(buffer, anonymous: source);
```

[PC]:https://www.nuget.org/packages/Mikodev.Binary/
[PF]:https://www.nuget.org/packages/Mikodev.Binary.FSharp/
[VC]:https://img.shields.io/nuget/v/Mikodev.Binary
[VF]:https://img.shields.io/nuget/v/Mikodev.Binary.FSharp
[IC]:https://img.shields.io/nuget/dt/Mikodev.Binary
[IF]:https://img.shields.io/nuget/dt/Mikodev.Binary.FSharp
