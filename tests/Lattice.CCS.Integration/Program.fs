module Lattice.CCS.Integration

open System
open System.IO
open System.Security.Cryptography
open System.Text.Json
open Clef.Compiler.Editor
open Clef.Compiler.PSGSaturation.SemanticGraph.Types

let check condition message =
    if not condition then
        failwith message

let equal expected actual =
    check (expected = actual) $"Expected {expected}; got {actual}"

let current value =
    value
    |> Option.defaultWith (fun () -> failwith "The compiler check was superseded")

let prelude = "module OptionCorpus\n[<Measure>] type m\n[<Measure>] type s\n"

let source body =
    prelude + body + "\n[<EntryPoint>]\nlet main _ = ignore selected; 0\n"

let accepted =
    [
        "eager fallback", "let selected = Option.defaultValue 1<m> (Some 2<m>)"
        "lazy fallback", "let selected = Option.defaultWith (fun () -> 1<m>) None"
        "partial fallback", "let choose = Option.defaultWith (fun () -> 1<m>)\nlet selected = choose None"
        "function payload", "let selected = Option.defaultWith (fun () -> fun (x: int<m>) -> x) None 2<m>"
        "negative dimension", "let selected = Option.defaultWith (fun () -> 1<m^-1>) (Some (2 / 1<m>))"
        "fractional value", "let selected = Option.defaultWith (fun () -> -0.25<m^-1>) (Some (1.0 / 2.0<m>))"
    ]

// Markers identify the exact compiler diagnostic span; parser/project failures
// or a different diagnostic do not count as rejecting the intended invalid case.
let rejected =
    [
        "eager fallback dimension",
        "CCS8040",
        "let stored = Option.defaultValue 1<m>\nlet selected = «stored (Some 2<s>)»"
        "fallback dimension", "CCS8040", "let selected = «Option.defaultWith (fun () -> 1<m>) (Some 2<s>)»"
        "partial dimension",
        "CCS8040",
        "let choose = Option.defaultWith (fun () -> 1<m>)\nlet selected = «choose (Some 2<s>)»"
        "thunk argument", "CCS8003", "let selected = «Option.defaultWith (fun (_: int<m>) -> 1<m>)» None"
        "missing thunk", "CCS8003", "let selected = «Option.defaultWith 1<m>» None"
        "fractional dimension rejected", "CCS8048", "let selected = Option.defaultWith<float<«m^(1/2)»>>"
    ]

[<EntryPoint>]
let main _ =
    let root =
        Path.Combine(Path.GetTempPath(), "lattice-ccs-options-" + Guid.NewGuid().ToString("N"))

    Directory.CreateDirectory root |> ignore
    let file = Path.Combine(root, "Options.clef").Replace('\\', '/')
    let project = Path.Combine(root, "Options.fidproj")

    let manifest =
        """[package]
name = "lattice-option-corpus"
[compilation]
target = "library"
[build]
sources = ["Options.clef"]
output_kind = "library"
"""

    File.WriteAllText(project, manifest)
    let saved = source (snd accepted.Head)
    File.WriteAllText(file, saved)
    let session = EditorSession(project)

    let validateSnapshot (snapshot: EditorSnapshot) =
        check snapshot.Failure.IsNone $"Project check failed: {snapshot.Failure}"
        check snapshot.ParseFailures.IsEmpty $"Unexpected parse failures: {snapshot.ParseFailures}"

        check
            (snapshot.Diagnostics
             |> List.forall (fun d -> not (d.Code.StartsWith("IONIDE-"))))
            "An inherited analyzer supplied a compiler diagnostic"

        equal 64 snapshot.CompilerIdentity.Length

    let first = session.CheckAsync(Map.empty).Result |> current
    validateSnapshot first
    let firstText = JsonSerializer.Serialize first

    for name, body in accepted do
        let snapshot =
            session.CheckAsync(Map.ofList [ file, source body ]).Result |> current

        validateSnapshot snapshot

        check
            (snapshot.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error"))
            $"{name}: {snapshot.Diagnostics}"

        let selectedLine =
            (prelude + body).Split('\n')
            |> Array.findIndex (fun line -> line.StartsWith("let selected"))

        let hover = session.TryHover(snapshot.Revision, file, selectedLine, 5) |> current
        check (hover.Type.Contains("m")) $"{name}: lost dimensional type {hover.Type}"
        printfn "PASS CCS projection: %s" name

    for name, code, marked in rejected do
        let start = marked.IndexOf('«')
        let finish = marked.IndexOf('»')
        let before = prelude + marked.Substring(0, start)
        let through = before + marked.Substring(start + 1, finish - start - 1)

        let position (text: string) =
            let lines = text.Split('\n')
            lines.Length - 1, lines[lines.Length - 1].Length

        let line, column = position before
        let endLine, endColumn = position through

        let expected =
            {
                FilePath = file
                StartLine = line
                StartCharacter = column
                EndLine = endLine
                EndCharacter = endColumn
            }

        let body = marked.Replace("«", "").Replace("»", "")
        let priorRevision = session.Revision
        let work = session.CheckAsync(Map.ofList [ file, source body ])
        check (session.TryHover(priorRevision, file, 3, 5).IsNone) "An obsolete snapshot supplied a hover"
        let snapshot = work.Result |> current
        validateSnapshot snapshot

        let diagnostics =
            snapshot.Diagnostics
            |> List.filter (fun d -> d.Code = code && d.EffectiveSeverity = "Error")

        equal 1 diagnostics.Length
        let diagnostic = diagnostics.Head
        equal "Error" diagnostic.Severity
        equal (Some expected) diagnostic.Range
        check (not (String.IsNullOrWhiteSpace diagnostic.Message)) "Compiler diagnostic lost its explanation"
        printfn "PASS CCS rejection: %s (%s, exact source range)" name code

    let repaired = session.CheckAsync(Map.empty).Result |> current
    validateSnapshot repaired

    check
        (repaired.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error"))
        "Saved correction did not clear compiler errors"

    equal saved (File.ReadAllText file)
    equal firstText (JsonSerializer.Serialize first)
    check (repaired.Revision > first.Revision) "Snapshot revision did not advance"
    let compilerPath = typeof<SemanticNode>.Assembly.Location

    let compilerHash =
        Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes compilerPath))

    equal compilerHash repaired.CompilerIdentity

    let evidence =
        {|
            compiler = compilerPath
            compilerHash = compilerHash
            accepted = accepted |> List.map fst
            rejected = rejected |> List.map (fun (name, code, _) -> {| name = name; code = code |})
            firstRevision = first.Revision
            finalRevision = repaired.Revision
        |}

    File.WriteAllText(
        Path.Combine(root, "evidence.json"),
        JsonSerializer.Serialize(evidence, JsonSerializerOptions(WriteIndented = true))
    )

    printfn "PASS snapshot invalidation, unsaved repair and retained immutable evidence: %s" root
    0
