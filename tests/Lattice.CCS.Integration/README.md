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
Result defaults retain measured success types independently of the error type;
`defaultWith` takes that error payload and its partial signature exposes both
Result arguments. `Result.iter` retains a unit result for its measured action.
Wrong fallback/callback dimensions and nonunit actions require exact CCS8040 or
CCS8003 spans; unsaved repairs restore the corresponding result and partial hovers.
`Result.isOk` and `isError` project `bool`; their typed aliases retain independent
success and error dimensions in the Result argument.
Payload-dimension mismatches and predicate overapplication require exact
CCS8040/CCS8003 spans; repairs restore bool results and alias payload signatures.
Simple parenthesized integer ranges retain a unit result and an `int` induction
reference hover. Floating, Boolean and measured bounds require CCS8003/CCS8040
at the complete loop span, as in the compiler's `RangeLoopCases` fixtures.
An iteration-capturing lambda retains its `int -> unit` source signature; its
captured `int` reference resolves to the loop's source identifier. Counted/range
assignments require CCS8009 at the assigned value, and unsaved repair restores
the signature and source definition.
Unsupported computation bodies, seq bang bindings, yields inside ordinary
lambdas and lexically shadowed `seq` forms require CCS8401 at their complete
unsupported AST span. Repairs retain ordinary `Result.iter` and native `seq`
source hovers. This checks source admission only, not general builder support,
sequence-frame completion or sequence execution.
Nested sequence owners retain independent `seq<int<m>>` and `seq<bool>` hovers.
Mixed-dimension yields, scalar `yield!` operands and contradictory yield!-only
delegations require exact CCS8040/CCS8003 form spans. Unsaved repairs restore both
owner types; these remain source/type projections only.
For a sequence capturing a local measured seed, the source `seq` token still
projects `SeqExpr` and `seq<int<m>>`, not its internal generator formal. The
captured reference resolves to the seed's compiler-projected source declaration;
the factory keeps its `unit -> seq<int<m>>` signature.
Measured `Seq.filter`/`map` and `Seq.collect`/`append` compositions retain their
`seq<T>` binding and full-application result hovers. Generated operand snapshots
cannot occupy these source positions; callback captures resolve to the original
measured declaration. Wrong callback and delegation dimensions require exact
CCS8040 spans, and unsaved repairs restore the result and definition projections.
These checks establish compiler-to-editor parity, not sequence execution.
Ownership projections cover nested sequences, guarded effects and delegation,
plus an effectful `seq<unit>` body with no yield. Captured mutable reads navigate
to their original declarations. A yield escaping into an ordinary lambda or lazy
body requires CCS8401 at the yield form; repair restores the owner types and
source definitions. Raw suspension-hyperedge incidence is checked by CCS tests;
this gate reads the public source projections without adding an editor graph API.
An effectful `yield!` operand and nested `Seq.append`/`collect` composition retain
their measured sequence types and original capture definitions after delegation
elaboration. The source delegation keeps its exact range and unit-valued hover
as the compiler's sequential wrapper. Generated iterator loops are compiler graph
tests; these projections establish neither guarded execution nor exhaustion behavior.
A guarded-yield while loop and captured lambda/lazy values inside a sequence
retain measured result hovers, the lambda's source signature and original
capture definitions. CCS tests own `SequenceEvaluation` edge incidence,
including backedges and deferred-body boundaries. This gate adds no client
evaluation model and does not establish native suspension behavior.
Local module and record definitions of `Math.sin` retain their measured result
hover. These unsaved lexical definitions clear an intrinsic `Math.sin` dimensional
error, whose CCS8040 severity and full application span are checked exactly.

This is a compiler-consumer integration gate, not a ported analyzer or a language execution
oracle. The [waypoint](../../docs/option-waypoint.md) links the separately owned compiler,
LSP and source-to-native gates.
