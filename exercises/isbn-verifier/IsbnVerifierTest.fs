// This file was auto-generated based on version 2.0.0 of the canonical data.

module IsbnVerifierTest

open FsUnit.Xunit
open Xunit

open IsbnVerifier

[<Fact>]
let ``Valid isbn number`` () =
    isValid "3-598-21508-8" |> should equal true

[<Fact(Skip = "Remove to run test")>]
let ``Invalid isbn check digit`` () =
    isValid "3-598-21508-9" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Valid isbn number with a check digit of 10`` () =
    isValid "3-598-21507-X" |> should equal true

[<Fact(Skip = "Remove to run test")>]
let ``Check digit is a character other than X`` () =
    isValid "3-598-21507-A" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Invalid character in isbn`` () =
    isValid "3-598-2K507-0" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``X is only valid as a check digit`` () =
    isValid "3-598-2X507-9" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Valid isbn without separating dashes`` () =
    isValid "3598215088" |> should equal true

[<Fact(Skip = "Remove to run test")>]
let ``Isbn without separating dashes and X as check digit`` () =
    isValid "359821507X" |> should equal true

[<Fact(Skip = "Remove to run test")>]
let ``Isbn without check digit and dashes`` () =
    isValid "359821507" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Too long isbn and no dashes`` () =
    isValid "3598215078X" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Isbn without check digit`` () =
    isValid "3-598-21507" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Too long isbn`` () =
    isValid "3-598-21507-XX" |> should equal false

[<Fact(Skip = "Remove to run test")>]
let ``Check digit of X should not be used for 0`` () =
    isValid "3-598-21515-X" |> should equal false

