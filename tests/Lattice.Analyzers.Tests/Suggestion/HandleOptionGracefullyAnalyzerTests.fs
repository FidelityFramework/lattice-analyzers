module Lattice.Analyzers.Tests.Suggestion.HandleOptionGracefullyAnalyzerTests

open NUnit.Framework
open FSharp.Compiler.CodeAnalysis
open FSharp.Analyzers.SDK.Testing
open Lattice.Analyzers.Suggestion.HandleOptionGracefullyAnalyzer

let mutable projectOptions: FSharpProjectOptions = FSharpProjectOptions.zero

let messageString =
    "Handle both cases instead of unwrapping a potentially absent option value."

[<SetUp>]
let Setup () =
    task {
        let! opts = mkOptionsFromProject "net10.0" []
        projectOptions <- opts
    }

[<Test>]
let ``Option.get is detected`` () =
    async {
        let source =
            """
module M

let option = Some 10
let value = Option.get option
    """

        let ctx = getContext projectOptions source
        let! msgs = optionGetCliAnalyzer ctx
        Assert.That(msgs, Is.Not.Empty)
        Assert.That(Assert.messageContains messageString msgs[0], Is.True)
    }

[<Test>]
let ``ValueOption.get is detected`` () =
    async {
        let source =
            """
module M

let voption = ValueSome 10
let value = ValueOption.get voption
    """

        let ctx = getContext projectOptions source
        let! msgs = optionGetCliAnalyzer ctx
        Assert.That(msgs, Is.Not.Empty)
        Assert.That(Assert.messageContains messageString msgs[0], Is.True)
    }

[<Test>]
let ``Option.Value member is detected`` () =
    async {
        let source =
            """
module M

let option = Some 10
let value = option.Value
    """

        let ctx = getContext projectOptions source
        let! msgs = optionGetCliAnalyzer ctx
        Assert.That(msgs, Is.Not.Empty)
        Assert.That(Assert.messageContains messageString msgs[0], Is.True)
    }

[<Test>]
let ``ValueOption.Value member is detected`` () =
    async {
        let source =
            """
module M

let voption = ValueSome 10
let value = voption.Value
    """

        let ctx = getContext projectOptions source
        let! msgs = optionGetCliAnalyzer ctx
        Assert.That(msgs, Is.Not.Empty)
        Assert.That(Assert.messageContains messageString msgs[0], Is.True)
    }

[<TestCase("Option.defaultValue 10 (Some 20)")>]
[<TestCase("Option.defaultWith (fun () -> 10) None")>]
[<TestCase("Some 20 |> Option.defaultValue 10")>]
[<TestCase("None |> Option.defaultWith (fun () -> 10)")>]
let ``Fallback combinators are not option unwrapping`` expression =
    async {
        let ctx = getContext projectOptions $"module M\nlet value = {expression}\n"
        Assert.That(ctx.CheckFileResults.HasFullTypeCheckInfo, Is.True)

        Assert.That(
            ctx.CheckFileResults.Diagnostics
            |> Array.filter (fun d -> d.Severity = FSharp.Compiler.Diagnostics.FSharpDiagnosticSeverity.Error),
            Is.Empty
        )

        let! messages = optionGetCliAnalyzer ctx
        Assert.That(messages, Is.Empty)
    }

[<Test>]
let ``User Option get is identified by its resolved symbol`` () =
    async {
        let source =
            """module M
module Option =
    let get value = value + 1
let value = Option.get 10
"""

        let ctx = getContext projectOptions source
        Assert.That(ctx.CheckFileResults.HasFullTypeCheckInfo, Is.True)

        Assert.That(
            ctx.CheckFileResults.Diagnostics
            |> Array.filter (fun d -> d.Severity = FSharp.Compiler.Diagnostics.FSharpDiagnosticSeverity.Error),
            Is.Empty
        )

        let! messages = optionGetCliAnalyzer ctx
        Assert.That(messages, Is.Empty)
    }

[<Test>]
let ``Unwrapping diagnostic preserves identity and does not invent an effect changing fix`` () =
    async {
        let source = "module M\nlet value = Option.get (Some 10)\n"
        let ctx = getContext projectOptions source
        let! messages = optionGetCliAnalyzer ctx
        Assert.That(messages, Has.Length.EqualTo(1))
        let message = List.exactlyOne messages
        Assert.That(message.Code, Is.EqualTo("IONIDE-006"))
        Assert.That(message.Severity, Is.EqualTo(FSharp.Analyzers.SDK.Severity.Warning))
        Assert.That(message.Range.StartLine, Is.EqualTo(2))
        Assert.That(message.Range.StartColumn, Is.EqualTo(12))
        Assert.That(message.Fixes, Is.Empty)
    }
