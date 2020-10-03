namespace Miscellaneous

open Mikodev.Binary
open System
open Xunit

type Group =
    | I0
    | I1 of int
    | I2 of string * int
    | I3 of single * string * double
    | I4 of string * int16 * string * int64

type CrossTests () =
    let generator =
        Generator.CreateDefaultBuilder()
            .AddFSharpConverterCreators()
            .Build();

    static member ``Data Alpha`` : (obj array) seq = seq {
        yield [| I0; 0uy |]
        yield [| I1 100; (1uy, 100) |]
        yield [| I2 ("two",  255); (2uy, "two", 255) |]
        yield [| I3 (1.1f, "three", 2.2); (3uy, 1.1f, "three", 2.2) |]
        yield [| I4 ("fox", -1s, "dog", 5L); (4uy, "fox", -1s, "dog", 5L) |]
    }

    [<Theory>]
    [<MemberData("Data Alpha")>]
    member __.``Union And Tuple`` (a : Group, b : 'B) =
        let alpha = generator.Encode a
        let bravo = generator.Encode b
        Assert.Equal<byte>(alpha, bravo)
        ()

    static member ``Data Bravo`` : (obj array) seq = seq {
        yield [| I1 127 |]
        yield [| I3 (0.3f, "let", 2.6) |]
    }

    [<Theory>]
    [<MemberData("Data Bravo")>]
    member __.``Invalid Union Case Type`` (a : 'A) =
        let message = sprintf "Invalid union type, you may have to use union type '%O' instead of case type '%O'" typeof<Group> typeof<'A>
        let error = Assert.Throws<ArgumentException>(fun () -> generator.Encode a |> ignore)
        Assert.Equal(message, error.Message)
        ()
