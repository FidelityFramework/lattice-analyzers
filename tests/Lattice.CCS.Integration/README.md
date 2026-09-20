# CCS compiler surface projection gate

This executable checks Option and direct immutable capture projections through **CCS.Editor** in the sibling
Composer checkout. It does not load the inherited FCS analyzers or implement new semantics.
It is separate from the standalone analyzer solution because it requires aligned Composer and
clef checkouts and their compiler build.

```sh
dotnet run --project tests/Lattice.CCS.Integration/Lattice.CCS.Integration.fsproj
```

Override `CCSEditorProject` when Composer is elsewhere. Composer's normal
`ClefCompilerServiceProject` selection still governs its compiler reference. When sharing a
workspace, coordinate builds with other compiler work; after the dependencies have been built
in the same configuration, `-p:BuildProjectReferences=false` avoids rebuilding their outputs.

The gate requires successful project loading and parsing and checks accepted dimensional Option
values and hovers. Captured local function declarations and references retain their exact source
arity and dimensional type, alongside a captureless control and returned-function result. A
captured reference must still resolve to the original declaration, not a generated formal. A wrong
explicit argument requires CCS8040 with Error severity at its exact source span; the Option
negative cases retain their existing checks. It also checks obsolete-revision rejection,
unsaved edit repair, unchanged disk source
and immutable retained snapshots. A temporary `evidence.json` records the loaded compiler path,
its SHA-256 identity, cases and revisions. The executable exits nonzero on any failed assertion.

This is a compiler-consumer integration gate, not a ported analyzer or a language execution
oracle. The [waypoint](../../docs/option-waypoint.md) links the separately owned compiler,
LSP and source-to-native gates.
