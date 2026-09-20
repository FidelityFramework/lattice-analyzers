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

let loopCapture =
    "let selected =\n    for index = 1 to 2 do\n        let visit = fun (value: int) -> ignore (index + value)\n        visit 0\n    ()"

let nestedSequence =
    "let selected = seq {\n    let inner = seq { yield true }\n    yield 1<m>\n}"

let capturedSequence =
    "let make () =\n    let seed = 1<m>\n    seq { yield seed }\nlet selected = make ()"

// Shared source contract with Composer/tests/CCS.Editor.Tests/Program.fs.
let directCaptures =
    """module DirectCaptures
[<Measure>] type m
[<Measure>] type s
[<EntryPoint>]
let main _ =
    let offset = 7<m>
    let shift (value: int<m>) = offset + value
    let plain (value: int<m>) = value
    let make () = fun (value: int<m>) -> offset + value
    let shifted = shift 3<m>
    let unchanged = plain 10<m>
    let produced = make () 3<m>
    if shifted = unchanged && produced = 10<m> then 0 else 1
"""

let accepted =
    [
        "eager fallback", "let selected = Option.defaultValue 1<m> (Some 2<m>)"
        "lazy fallback", "let selected = Option.defaultWith (fun () -> 1<m>) None"
        "partial fallback", "let choose = Option.defaultWith (fun () -> 1<m>)\nlet selected = choose None"
        "function payload", "let selected = Option.defaultWith (fun () -> fun (x: int<m>) -> x) None 2<m>"
        "negative dimension", "let selected = Option.defaultWith (fun () -> 1<m^-1>) (Some (2 / 1<m>))"
        "fractional value", "let selected = Option.defaultWith (fun () -> -0.25<m^-1>) (Some (1.0 / 2.0<m>))"
        "orElse eager optional result", "let selected = Option.orElse (Some 1<m>) None"
        "orElseWith deferred optional result", "let selected = Option.orElseWith (fun () -> Some 2<m>) (Some 3<m>)"
        "orElse partial optional result", "let choose = Option.orElse (Some 4<m>)\nlet selected = choose None"
        "orElseWith partial optional result",
        "let choose = Option.orElseWith (fun () -> Some 5<m>)\nlet selected = choose None"
        "orElse bare alias optional result", "let choose = Option.orElse\nlet selected = choose None (Some 6<m>)"
        "orElseWith bare alias optional result",
        "let choose = Option.orElseWith\nlet selected = choose (fun () -> Some 7<m>) None"
        "iter unit result", "let selected = Option.iter (fun (value: int<m>) -> ignore value) (Some 1<m>)"
        "iter partial action",
        "let choose = Option.iter (fun (value: int<m>) -> ignore value)\nlet selected = choose None"
        "iter bare generic alias",
        "let choose = Option.iter\nlet first = choose (fun (value: int<m>) -> ignore value) (Some 2<m>)\nlet selected = choose (fun (value: int<s>) -> ignore value) (Some 3<s>)"
        "fold partial independent dimensions",
        "let choose = Option.fold<int<m>, int<s>> (fun state value -> state) 1<m>\nlet selected = choose (Some 2<s>)"
        "foldBack partial independent dimensions",
        "let choose = Option.foldBack<int<m>, int<s>> (fun value state -> state) (Some 2<s>)\nlet selected = choose 1<m>"
        "fold bare aliases",
        "let forward = Option.fold\nlet backward = Option.foldBack\nlet first = forward (fun (state: int<m>) (_: int<s>) -> state) 1<m> (Some 2<s>)\nlet selected = backward (fun (_: int<s>) (state: int<m>) -> state) (Some 3<s>) first"
        "Result.map partial changes success dimension",
        "[<Measure>] type kg\nlet choose = Result.map<int<m>, int<kg>, int<s>> (fun _ -> 1<kg>)\nlet selected = choose (Ok 2<m>)"
        "Result.mapError changes error dimension",
        "[<Measure>] type kg\nlet selected = Result.mapError<int<m>, int<s>, int<kg>> (fun _ -> 1<kg>) (Error 2<s>)"
        "Result.bind preserves error dimension",
        "[<Measure>] type kg\nlet selected = Result.bind<int<m>, int<kg>, int<s>> (fun _ -> Ok 1<kg>) (Ok 2<m>)"
        "Result.defaultValue measured success",
        "let selected = Result.defaultValue<int<m>, int<s>> 1<m> (Error 2<s>)"
        "Result.defaultWith partial error handler",
        "let choose = Result.defaultWith<int<m>, int<s>> (fun error -> error * 1<m> / 1<s>)\nlet selected = choose (Error 2<s>)"
        "Result.iter measured action",
        "let selected = Result.iter<int<m>, int<s>> (fun value -> ignore value) (Ok 2<m>)"
        "Result.isOk measured alias",
        "let inspect = Result.isOk<int<m>, int<s>>\nlet selected = inspect (Ok 1<m>)"
        "Result.isError measured alias",
        "let inspect = Result.isError<int<s>, int<m>>\nlet selected = inspect (Error 2<m>)"
        "native seq source", "let selected = seq { yield 1<m> }"
        "nested sequence element owners", nestedSequence
        "captured sequence source identity", capturedSequence
        "integer range loop",
        "let selected =\n    for index in (-2 .. 2) do ignore index\n    ()"
        "loop capture source signature", loopCapture
    ]

// Lexical declarations own these names; neither is the dimensionless intrinsic.
let lexicalMath =
    [
        "local Math module",
        "module Math =\n    let sin (value: int<m>) = value\nlet selected = Math.sin 2<m>"
        "local Math record",
        "type Functions = { sin: int<m> -> int<m> }\nlet Math = { sin = fun value -> value }\nlet selected = Math.sin 2<m>"
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
        "orElse fallback option dimension", "CCS8040", "let selected = «Option.orElse (Some 1<m>) (Some 2<s>)»"
        "orElse partial dimension",
        "CCS8040",
        "let choose = Option.orElse (Some 1<m>)\nlet selected = «choose (Some 2<s>)»"
        "orElse nonoption fallback", "CCS8003", "let selected = «Option.orElse 1<m>» None"
        "orElseWith fallback option dimension",
        "CCS8040",
        "let selected = «Option.orElseWith (fun () -> Some 1<m>) (Some 2<s>)»"
        "orElseWith partial dimension",
        "CCS8040",
        "let choose = Option.orElseWith (fun () -> Some 1<m>)\nlet selected = «choose (Some 2<s>)»"
        "orElseWith thunk domain", "CCS8003", "let selected = «Option.orElseWith (fun (_: int<m>) -> Some 1<m>)» None"
        "orElseWith nonoption thunk result", "CCS8003", "let selected = «Option.orElseWith (fun () -> 1<m>)» None"
        "iter nonunit callback result", "CCS8003", "let selected = «Option.iter (fun (value: int<m>) -> value)» None"
        "iter argument dimension", "CCS8040", "let selected = «Option.iter (fun (_: int<m>) -> ()) (Some 1<s>)»"
        "iter nonoption input", "CCS8003", "let selected = «Option.iter (fun (_: int<m>) -> ()) 1<m>»"
        "iter nonfunction callback", "CCS8003", "let selected = «Option.iter 1<m>» None"
        "direct capture explicit argument dimension",
        "CCS8040",
        "let selected =\n    let offset = 7<m>\n    let shift (value: int<m>) = offset + value\n    «shift 3<s>»"
        "fold callback state dimension", "CCS8040",
        "let selected = «Option.fold (fun (state: int<m>) (_: int<s>) -> state) 1<s>» None"
        "foldBack callback payload dimension", "CCS8040",
        "let selected = «Option.foldBack (fun (_: int<s>) (state: int<m>) -> state) (Some 2<m>)» 1<m>"
        "fold callback result dimension", "CCS8040",
        "let selected = «Option.fold (fun (state: int<m>) (_: int<s>) -> 1<s>)» 1<m> None"
        "Result.map success dimension", "CCS8040",
        "let selected = «Result.map (fun (_: int<m>) -> true) (Ok 2<s>: Result<int<s>, bool>)»"
        "Result.mapError error dimension", "CCS8040",
        "let selected = «Result.mapError (fun (_: int<m>) -> true) (Error 2<s>: Result<bool, int<s>>)»"
        "Result.bind shared error dimension", "CCS8040",
        "let selected = «Result.bind (fun (_: bool) -> (Error 3<m>: Result<bool, int<m>>)) (Error 2<s>: Result<bool, int<s>>)»"
        "Result.defaultValue fallback dimension", "CCS8040",
        "let selected = «Result.defaultValue 1<m> (Ok 2<s>: Result<int<s>, bool>)»"
        "Result.defaultWith error callback dimension", "CCS8040",
        "let selected = «Result.defaultWith (fun (_: int<s>) -> 1<m>) (Error 2<m>: Result<int<m>, int<m>>)»"
        "Result.iter nonunit action", "CCS8003",
        "let selected = «Result.iter (fun (_: bool) -> 3)» (Ok true: Result<bool, bool>)"
        "Result.isOk payload dimension", "CCS8040",
        "let selected = «Result.isOk<int<m>, bool> (Ok 3<s>)»"
        "Result.isError overapplication", "CCS8003",
        "let selected = «Result.isError (Error (): Result<int<m>, unit>) true»"
        "range loop floating bound", "CCS8003",
        "let selected =\n    «for index in 0.0 .. 1 do ignore index»\n    ()"
        "range loop Boolean bound", "CCS8003",
        "let selected =\n    «for index in true .. 1 do ignore index»\n    ()"
        "range loop measured bound", "CCS8040",
        "let selected =\n    «for index in 1<m> .. 3 do ignore index»\n    ()"
        "immutable loop counted assignment", "CCS8009",
        "let selected =\n    for index = 1 to 3 do\n        index <- «9»\n    ()"
        "immutable loop range assignment", "CCS8009",
        "let selected =\n    for index in 1 .. 3 do\n        index <- «9»\n    ()"
        "CE custom builder", "CCS8401",
        "let builder value = value\nlet selected = builder «{\n    return true\n}»"
        "CE seq let bang", "CCS8401",
        "let selected = seq {\n    «let! value = true\n    yield value»\n}"
        "CE seq lambda yield", "CCS8401",
        "let selected = seq {\n    let work = fun () -> «yield 1»\n    yield 2\n}"
        "CE lexical seq", "CCS8401",
        "let seq value = not value\nlet selected = seq «{ yield true }»"
        "Sequence mixed yield dimensions", "CCS8040",
        "let selected = seq { yield 1<m>; «yield 2<s>» }"
        "Sequence scalar delegation", "CCS8003",
        "let selected = seq { «yield! 1» }"
        "Sequence conflicting delegations", "CCS8040",
        "let selected = seq { yield! seq { yield 1<m> }; «yield! seq { yield 2<s> }» }"
        "intrinsic Math.sin dimension", "CCS8040", "let selected = «Math.sin 1.0<m>»"
    ]

[<EntryPoint>]
let main _ =
    let root =
        Path.Combine(Path.GetTempPath(), "lattice-ccs-surface-" + Guid.NewGuid().ToString("N"))

    Directory.CreateDirectory root |> ignore
    let file = Path.Combine(root, "Surface.clef").Replace('\\', '/')
    let project = Path.Combine(root, "Surface.fidproj")

    let manifest =
        """[package]
name = "lattice-compiler-surface-corpus"
[compilation]
target = "library"
[build]
sources = ["Surface.clef"]
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

    let checkCapturedSequence (snapshot: EditorSnapshot) =
        let lines = (source capturedSequence).Split('\n')
        let hoverAt (marker: string) (token: string) =
            let line = lines |> Array.findIndex (fun text -> text.Contains(marker, StringComparison.Ordinal))
            let column = lines[line].IndexOf(token, StringComparison.Ordinal)
            session.TryHover(snapshot.Revision, file, line, column) |> current
        equal "unit -> seq<int<m>>" (hoverAt "let make" "make").Type
        equal "seq<int<m>>" (hoverAt "let selected" "selected").Type
        let expression = hoverAt "seq { yield seed }" "seq"
        equal "seq<int<m>>" expression.Type
        equal "SeqExpr" expression.Kind
        let declaration = hoverAt "let seed" "seed"
        let captured = hoverAt "seq { yield seed }" "seed"
        equal "int<m>" declaration.Type
        equal "int<m>" captured.Type
        equal "Binding" declaration.Kind
        equal "VarRef" captured.Kind
        equal (Some declaration.Range) captured.Definition
        equal (lines |> Array.findIndex (fun text -> text.Contains("let seed", StringComparison.Ordinal))) declaration.Range.StartLine

    let checkNestedSequence (snapshot: EditorSnapshot) =
        let lines = (source nestedSequence).Split('\n')
        for name, expected in ["selected", "seq<int<m>>"; "inner", "seq<bool>"] do
            let line = lines |> Array.findIndex (fun text -> text.Contains("let " + name + " =", StringComparison.Ordinal))
            let column = lines[line].IndexOf(name, StringComparison.Ordinal)
            equal expected (session.TryHover(snapshot.Revision, file, line, column) |> current).Type

    let checkLoopCapture (snapshot: EditorSnapshot) =
        let lines = (source loopCapture).Split('\n')
        let hoverAt (marker: string) (name: string) reference =
            let line = lines |> Array.findIndex (fun text -> text.Contains(marker, StringComparison.Ordinal))
            let column = if reference then lines[line].LastIndexOf(name, StringComparison.Ordinal) else lines[line].IndexOf(name, StringComparison.Ordinal)
            session.TryHover(snapshot.Revision, file, line, column) |> current
        equal "unit" (hoverAt "let selected" "selected" false).Type
        equal "int -> unit" (hoverAt "let visit" "visit" false).Type
        equal "int -> unit" (hoverAt "visit 0" "visit" true).Type
        let capture = hoverAt "let visit" "index" true
        equal "int" capture.Type
        let line = lines |> Array.findIndex (fun text -> text.Contains("for index", StringComparison.Ordinal))
        let column = lines[line].IndexOf("index", StringComparison.Ordinal)
        equal (Some { FilePath = file; StartLine = line; StartCharacter = column; EndLine = line; EndCharacter = column + "index".Length }) capture.Definition

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

        if name = "captured sequence source identity" then
            checkCapturedSequence snapshot
        elif name = "nested sequence element owners" then
            checkNestedSequence snapshot
        elif name = "native seq source" then
            equal "seq<int<m>>" hover.Type
        elif name = "loop capture source signature" then
            checkLoopCapture snapshot
        elif name = "integer range loop" then
            equal "unit" hover.Type
            let lines = (prelude + body).Split('\n')
            let loopLine = lines |> Array.findIndex (fun line -> line.Contains("for index", StringComparison.Ordinal))
            let indexColumn = lines[loopLine].LastIndexOf("index", StringComparison.Ordinal)
            let induction = session.TryHover(snapshot.Revision, file, loopLine, indexColumn) |> current
            equal "int" induction.Type
            equal "VarRef" induction.Kind
        elif name.StartsWith("Result.", StringComparison.Ordinal) then
            let expected =
                if name.StartsWith("Result.is", StringComparison.Ordinal) then "bool"
                elif name.StartsWith("Result.default", StringComparison.Ordinal) then "int<m>"
                elif name.StartsWith("Result.iter", StringComparison.Ordinal) then "unit"
                elif name.StartsWith("Result.mapError", StringComparison.Ordinal) then "Result<int<m>, int<kg>>"
                else "Result<int<kg>, int<s>>"
            equal expected hover.Type

            if name.StartsWith("Result.is", StringComparison.Ordinal) then
                let alias = session.TryHover(snapshot.Revision, file, 3, 5) |> current
                let expectedAlias = if name.StartsWith("Result.isOk", StringComparison.Ordinal) then "Result<int<m>, int<s>> -> bool" else "Result<int<s>, int<m>> -> bool"
                equal expectedAlias alias.Type

            if name.Contains("partial", StringComparison.Ordinal) then
                let partialLine = (prelude + body).Split('\n') |> Array.findIndex (fun line -> line.StartsWith("let choose"))
                let partial = session.TryHover(snapshot.Revision, file, partialLine, 5) |> current
                let expectedPartial =
                    if name.StartsWith("Result.defaultWith", StringComparison.Ordinal) then "Result<int<m>, int<s>> -> int<m>"
                    else "Result<int<m>, int<s>> -> Result<int<kg>, int<s>>"
                equal expectedPartial partial.Type
        elif name.StartsWith("iter", StringComparison.Ordinal) then
            equal "unit" hover.Type

            if name = "iter partial action" then
                let partial = session.TryHover(snapshot.Revision, file, 3, 5) |> current
                equal "int<m> option -> unit" partial.Type
        else
            check (hover.Type.Contains("m")) $"{name}: lost dimensional type {hover.Type}"

        if name.StartsWith("orElse", StringComparison.Ordinal) then
            equal "int<m> option" hover.Type

            if name.Contains("partial", StringComparison.Ordinal) then
                let partial = session.TryHover(snapshot.Revision, file, 3, 5) |> current
                equal "int<m> option -> int<m> option" partial.Type

        if name.StartsWith("fold", StringComparison.Ordinal) then
            equal "int<m>" hover.Type

            if name.Contains("partial", StringComparison.Ordinal) then
                let partial = session.TryHover(snapshot.Revision, file, 3, 5) |> current
                let expected = if name.StartsWith("foldBack", StringComparison.Ordinal) then "int<m> -> int<m>" else "int<s> option -> int<m>"
                equal expected partial.Type

        printfn "PASS CCS projection: %s" name

    let captureSnapshot =
        session.CheckAsync(Map.ofList [ file, directCaptures ]).Result |> current

    validateSnapshot captureSnapshot

    check
        (captureSnapshot.Diagnostics
         |> List.forall (fun d -> d.EffectiveSeverity <> "Error"))
        $"Direct capture fixture has errors: {captureSnapshot.Diagnostics}"

    let lines = directCaptures.Split('\n')

    let captureHover (marker: string) (name: string) reference =
        let line =
            lines
            |> Array.findIndex (fun text -> text.Contains(marker, StringComparison.Ordinal))

        let column =
            if reference then
                lines[line].LastIndexOf(name, StringComparison.Ordinal)
            else
                lines[line].IndexOf(name, StringComparison.Ordinal)

        session.TryHover(captureSnapshot.Revision, file, line, column) |> current

    let captureSignatures =
        [
            "shift", "int<m> -> int<m>", "let shift", "let shifted = shift"
            "plain", "int<m> -> int<m>", "let plain", "let unchanged = plain"
            "make", "unit -> int<m> -> int<m>", "let make", "let produced = make"
        ]
        |> List.map (fun (name, signature, declaration, reference) ->
            let defined = captureHover declaration name false
            let used = captureHover reference name true
            equal signature defined.Type
            equal signature used.Type
            equal "VarRef" used.Kind
            equal defined.Range (used.Definition |> current)

            {|
                name = name
                signature = signature
                declaration = defined.Range
                reference = used.Range
            |}
        )

    let produced = captureHover "let produced" "produced" false
    equal "int<m>" produced.Type
    let offsetDeclaration = captureHover "let offset" "offset" false
    let capturedOffset = captureHover "let shift" "offset" true
    equal "int<m>" capturedOffset.Type
    equal offsetDeclaration.Range (capturedOffset.Definition |> current)
    printfn "PASS CCS projection: direct capture source signatures, captureless control and returned function result"

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

        equal
            1
            (snapshot.Diagnostics
             |> List.filter (fun d -> d.EffectiveSeverity = "Error")
             |> List.length)

        equal 1 diagnostics.Length
        let diagnostic = diagnostics.Head
        equal "Error" diagnostic.Severity
        equal (Some expected) diagnostic.Range
        check (not (String.IsNullOrWhiteSpace diagnostic.Message)) "Compiler diagnostic lost its explanation"
        printfn "PASS CCS rejection: %s (%s, exact source range)" name code

        if name.StartsWith("Sequence ", StringComparison.Ordinal) then
            let restored = session.CheckAsync(Map.ofList [ file, source nestedSequence ]).Result |> current
            validateSnapshot restored
            check (restored.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error")) "Sequence repair retained an error"
            checkNestedSequence restored
            printfn "PASS CCS independent sequence element types after unsaved repair: %s" name

        if name.StartsWith("CE ", StringComparison.Ordinal) then
            let repairName, expectedType =
                if name = "CE custom builder" then "Result.iter measured action", "unit"
                else "native seq source", "seq<int<m>>"
            let _, body = accepted |> List.find (fun (acceptedName, _) -> acceptedName = repairName)
            let text = source body
            let restored = session.CheckAsync(Map.ofList [ file, text ]).Result |> current
            validateSnapshot restored
            check (restored.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error")) "CE repair retained an error"
            let selectedLine = text.Split('\n') |> Array.findIndex (fun line -> line.StartsWith("let selected"))
            equal expectedType (session.TryHover(restored.Revision, file, selectedLine, 5) |> current).Type
            printfn "PASS CCS admitted source after unsaved repair: %s" name

        if name.StartsWith("Result.default", StringComparison.Ordinal) || name.StartsWith("Result.iter", StringComparison.Ordinal) || name.StartsWith("Result.is", StringComparison.Ordinal) then
            let operation = name.Split(' ')[0]
            let _, body = accepted |> List.find (fun (acceptedName, _) -> acceptedName.StartsWith(operation, StringComparison.Ordinal))
            let text = source body
            let restored = session.CheckAsync(Map.ofList [ file, text ]).Result |> current
            validateSnapshot restored
            check (restored.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error")) "Result repair retained an error"
            let lines = text.Split('\n')
            let selectedLine = lines |> Array.findIndex (fun line -> line.StartsWith("let selected"))
            let expectedType = if operation.StartsWith("Result.is", StringComparison.Ordinal) then "bool" elif operation = "Result.iter" then "unit" else "int<m>"
            equal expectedType (session.TryHover(restored.Revision, file, selectedLine, 5) |> current).Type
            if operation = "Result.defaultWith" then
                let partialLine = lines |> Array.findIndex (fun line -> line.StartsWith("let choose"))
                equal "Result<int<m>, int<s>> -> int<m>" (session.TryHover(restored.Revision, file, partialLine, 5) |> current).Type
            if operation.StartsWith("Result.is", StringComparison.Ordinal) then
                let aliasLine = lines |> Array.findIndex (fun line -> line.StartsWith("let inspect"))
                let expectedAlias = if operation = "Result.isOk" then "Result<int<m>, int<s>> -> bool" else "Result<int<s>, int<m>> -> bool"
                equal expectedAlias (session.TryHover(restored.Revision, file, aliasLine, 5) |> current).Type
            printfn "PASS CCS Result projection after unsaved repair: %s" operation

        if name.StartsWith("immutable loop", StringComparison.Ordinal) then
            check (diagnostic.Message.Contains("not found or not mutable", StringComparison.Ordinal)) "Loop assignment lost its immutable-binding explanation"
            let restored = session.CheckAsync(Map.ofList [ file, source loopCapture ]).Result |> current
            validateSnapshot restored
            check (restored.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error")) "Loop capture repair retained an error"
            checkLoopCapture restored
            printfn "PASS CCS loop signature and source definition after unsaved repair: %s" name

    // Repair the intrinsic error with real lexical definitions in unsaved source.
    let lexicalMathRepairs =
        lexicalMath
        |> List.map (fun (name, body) ->
            let text = source body
            let snapshot = session.CheckAsync(Map.ofList [ file, text ]).Result |> current
            validateSnapshot snapshot
            check
                (snapshot.Diagnostics |> List.forall (fun d -> d.EffectiveSeverity <> "Error"))
                $"{name}: {snapshot.Diagnostics}"
            let selectedLine = text.Split('\n') |> Array.findIndex (fun line -> line.StartsWith("let selected"))
            let hover = session.TryHover(snapshot.Revision, file, selectedLine, 5) |> current
            equal "int<m>" hover.Type
            printfn "PASS CCS projection and unsaved repair: %s" name
            {| name = name; source = text; revision = snapshot.Revision; result = hover.Type |})

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
            accepted = accepted @ lexicalMath |> List.map fst
            scope =
                "Option/Result, integer ranges, direct immutable capture and lexical Math projections through CCS.Editor; no analyzer rule or native execution claim"
            lexicalMathRepairs = lexicalMathRepairs
            loopCaptureSource = source loopCapture
            nestedSequenceSource = source nestedSequence
            capturedSequenceSource = source capturedSequence
            directCaptures =
                {|
                    source = directCaptures
                    revision = captureSnapshot.Revision
                    signatures = captureSignatures
                    capturedDefinition = capturedOffset.Definition
                    result = produced.Type
                |}
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
