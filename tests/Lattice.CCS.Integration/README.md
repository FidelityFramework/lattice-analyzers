# CCS compiler surface projection gate

This executable checks Option/Result and direct immutable capture projections through **CCS.Editor** in the sibling
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

The optional fallback corpus covers `Option.orElse` and `Option.orElseWith` through
direct, partial and bare-alias applications. Their result hovers must remain
`int<m> option`; partial hovers retain both option domains/results. Mismatched
dimensions, nonoption fallbacks/results and nonunit thunk arguments require the
existing CCS8040/CCS8003 errors at their exact source spans.
`Option.iter` checks unit results, a measured partial-action signature and one
bare alias used at two dimensions, with exact rejection of nonunit callback results,
wrong argument dimensions, nonoption inputs and nonfunction callbacks.
`Option.fold` and `Option.foldBack` keep state and payload dimensions independent,
including explicit state/payload type arguments and bare aliases. Their partial
hovers expose the remaining option or state argument respectively; invalid
callback state, payload and result types require exact compiler diagnostics.
`Result.map`, `mapError` and `bind` track success and error dimensions separately:
changing one preserves the other. Exact result and stored-partial hovers retain
both type arguments, including their declared explicit generic order.
Callback payload mismatches and a bind callback that changes the error dimension
require CCS8040 at the exact application span.
Simple parenthesized integer ranges retain a unit result and an `int` induction
reference hover. Floating, Boolean and measured bounds require CCS8003/CCS8040
at the complete loop span, as in the compiler's `RangeLoopCases` fixtures.
Local module and record definitions of `Math.sin` retain their measured result
hover. These unsaved lexical definitions clear an intrinsic `Math.sin` dimensional
error, whose CCS8040 severity and full application span are checked exactly.

This is a compiler-consumer integration gate, not a ported analyzer or a language execution
oracle. The [waypoint](../../docs/option-waypoint.md) links the separately owned compiler,
LSP and source-to-native gates.
