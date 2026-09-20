module Lattice.CCS.ProgramLifetime

open System
open System.IO

[<EntryPoint>]
let main args =
    let fixture =
        match args with
        | [||] -> Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "../../../Composer/tests/Fixtures/ProgramLifetime"))
        | [|path|] -> Path.GetFullPath path
        | _ -> invalidArg "args" "Expected an optional Composer program-lifetime fixture directory"
    let evidence = Path.Combine(Path.GetTempPath(), "lattice-program-lifetime-" + Guid.NewGuid().ToString("N"))
    try ProgramLifetimeProjection.run fixture evidence; 0
    with error -> eprintfn "%O\nEvidence: %s" error evidence; 1
