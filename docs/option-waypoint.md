# Option compiler and tooling waypoint

`Option.defaultValue` selects an already evaluated fallback. `Option.defaultWith` receives a
thunk and invokes it only for `None`; creating the thunk and evaluating the option are still
ordinary argument evaluations. Advice must preserve that distinction, dimensional identity,
function payloads and partial application. Neither combinator introduces null or an object carrier.

The semantic implementation belongs to CCS's Option ingredients/recipes and saturation.
Alex pulls settled graph facts. The analyzer repository contributes regression purposes and
checks the immutable consumer contract; it does not reconstruct Option semantics or run the
inherited FCS rule over Clef source.

| Gate | Owner and evidence | Limit |
| --- | --- | --- |
| Inherited option advice | `Lattice.Analyzers.Tests`: FSharp.Core unwrap detection, resolved user-symbol distinction, fallback combinators, warning identity and no automatic fix | F# rule maintenance only; `IONIDE-006` is not a Clef diagnostic |
| Option semantics and obligations | [CCS service tests](../../clef/tests/Clef.Compiler.Service.Tests), including `OptionDefaultWithCases.fs` | Compiler-owned positive/negative and settled graph checks |
| Consumer projection | [Lattice.CCS.Integration](../tests/Lattice.CCS.Integration/README.md) | Real CCS.Editor checks, dimensional hovers, exact diagnostic codes/severities/ranges, immutable snapshots and unsaved repair; no execution claim |
| LSP transport | [Lattice client tests](../../lattice-vscode/client/test) | Current document version, diagnostics and hover through the live server |
| Native execution | [Composer NativeCallbacks](../../Composer/tests/NativeCallbacks/README.md), [FidelityHello](../../Composer/samples/console/FidelityHelloWorld) | Source through the real native pipeline; branch effects, evaluation order, payload invocation and exit results |

The projection gate includes eager/lazy fallback types, stored partial application, function
payloads, negative dimensional powers and fractional numeric values. Negative cases require
the specific CCS code and exact source span: a parse failure or unrelated error is not a pass.
Fractional measure-exponent admission follows the compiler's current contract; the test records
its current `CCS8048` rejection rather than silently accepting incomplete checking.

For a future Clef option advisory, prerequisites are resolved operation identity, argument and
payload types, source range, effects/evaluation order, and the relevant checked snapshot.
Current CCS.Editor does not expose all of those as an advisory API. Missing or stale facts
therefore mean no justified rewrite, not a suggestion reconstructed from spelling or source text.
Required type/proof failures remain CCS diagnostics whether or not an advisory is enabled.

The live server uses Composer's CCS reference. ClefAutoComplete is the retained protocol/API
reference repository; Lattice.Server is the active implementation. The active VS Code and
Vim/Neovim clients consume that server. The VS Code helpers remain an inherited Fable library
for separate compatibility review; the thin client does not depend on them. The inherited SDK package is not
loaded into this path. The [shared integration plan](../../Composer/docs/Lattice_Integration.md)
owns the compiler/server boundary and remaining semantic-query work.

## C-06 native implementation waypoint — 2026-09-20

Compiler implementation:
[`12aa78d2b`](https://github.com/FidelityFramework/clef/commit/12aa78d2b),
with 848 CCS service cases passing. This remains an implementation waypoint
with the native regression gates below still open.

The same source projection path covers nested sequences whose inner owner
captures immutable and mutable bindings declared inside the outer sequence.
Their measured types and original declaration links remain visible. Counted
ascending and descending sequence bodies retain their immutable source induction
identities, and sequence consumption projects the current element type. A scalar
`for-in` input requires CCS8003 at the whole loop; unsaved repair restores the
element hover and source definition.

The tested source-projection CCS assembly is
`08d547524f7c76f61bb82e4a67e2f04ffe64b6ca37da7637ba2c4b2c07384482`.
The complete CCS.Editor gate passed **44 accepted and 46 exact rejected cases**,
alongside capture signature/definition checks, snapshot retention and repairs;
evidence: `/tmp/lattice-ccs-surface-a5571e6451fd49eeb0018150240a0396/evidence.json`.
The corresponding LSP gate passed **50 diagnostic edits and repairs** with the
same assembly; evidence: `/tmp/lattice-surface-waypoint-gTl1Dg/result.json`.

Composer's separate `15a_SequenceSemantics` oracle passed compilation, stock MLIR
verification, native execution and all ten exact output groups on final native
CCS `f4bbc287…432c1a`; evidence:
`/tmp/composer-native-sequences-d001fa03098148f8a94fb2fdf37454e2/evidence.json`.
The source gate prevents target frame synthesis
when effective source errors already exist, retaining their source graph and
diagnostics. The four affected CLI negatives passed unchanged on the
`08d54752…84482` assembly; the other twelve retain their preceding-build results. Evidence:
`/tmp/composer-source-admission-15540117b5aa4ec4928ea13d70ce4f38/evidence.json`
and `/tmp/composer-source-admission-000edec5a3b74f4286abaac3d3dba11c/evidence.json`.
The native oracle covers delayed effects, counted/conditional suspension, empty sequences,
caller-owned factories, delegation, repeated enumeration and retained nested
mutable captures. Baker owns the control, storage, origin and prerequisite
relations; Alex reads the settled graph. These executions and resident evidence
do not constitute blanket proof discharge or admit escaping storage without its
covering region. Required source and lifetime failures remain compiler gates.
The analyzer and LSP results establish source projection, not additional runtime
semantics. No inherited analyzer or retired ClefAutoComplete bridge was enabled.

The subsequent residence extension admits a captured sequence template only
when its source storage has a covering activation and explicit borrow incidence.
`15c_SequenceTemplateBorrows` passed fresh compilation, stock MLIR verification,
native execution and all three exact output groups on CCS
`f4bbc2879280b8252e3c7424a1b399e981eb492e49b07fb1def617d45f432c1a`;
evidence: `/tmp/composer-native-sequences-6f767e0ab8374941a1d041b0b9789985/evidence.json`.
Editor and server artifacts were refreshed to that assembly. The source
projection fixtures are unchanged and were not rerun after this native residence
extension; their `08d54752…84482` evidence above remains the tested record.

This is an implementation waypoint, not closure of the full C-06 gate. The
broader FidelityHello run on `08d54752…84482` compiled 23/28 samples and all 23
executed successfully, including 15a/15b. Five compile failures remain recorded:
05's float formatting carrier, 06's legacy integer conversion, 13's generic
integer width, 14's lazy width/extent, and the original 15's accumulating-frame
range bounds. The [Composer gate record](../../Composer/docs/Language_Coverage_Waypoints.md)
owns these remaining gates; the new sequence oracles do not replace them.
