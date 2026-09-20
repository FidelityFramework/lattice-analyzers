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
