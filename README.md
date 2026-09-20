# lattice-analyzers

A hard fork of [ionide-analyzers](https://github.com/ionide/ionide-analyzers), retained as a rule and regression
corpus and concrete analysis-slot inventory for Lattice. The twelve existing analyzers run over the F#
Compiler Service typed tree and emit `IONIDE-0nn` messages linking to ionide.io. They are not integrated with CCS.

The [shared Lattice integration plan](https://github.com/FidelityFramework/Composer/blob/main/docs/Lattice_Integration.md)
describes the implemented .NET-hosted [Lattice.Server](../Composer/src/Lattice.Server/README.md)
and immutable [CCS.Editor](../Composer/src/CCS.Editor/README.md) snapshots. CCS owns project source order,
checking, implemented position queries, diagnostics, and their evidence. Lattice clients display those
results; scope/completion queries and richer proof explanations remain separate compiler deliverables.
The .NET host runs the compiler service; it is not a fallback semantic
implementation and does not move type resolution or required proof checks out of CCS.

**First migration work.** Identify a concrete purpose and execution slot for each useful rule. Type resolution
and Baker carry and settle semantic information; required checks belong on that compiler path. If removing
an optional analyzer would allow ill-typed code or an unmet required proof to compile, the check cannot be
optional. Retiring the inherited FCS analyzer SDK path for Clef does not retire this repository or rule out
extensive proof-aware analysis. Its useful work is the rule inventory, prerequisites and test cases, with
semantic execution owned by the compiler.

Other slots can explain settled compiler facts, offer a style suggestion, or propose an additional property
for the user to consider. A proposed property is not an established fact. Once explicitly accepted as a
contract, it becomes a PSG obligation checked on the required compiler path. The
[analysis and analyzer slots](https://github.com/FidelityFramework/Composer/blob/main/docs/Lattice_Integration.md#analysis-and-analyzer-slots)
plan defines this placement; a generic plugin framework needs a concrete use case before it is justified.

For a selected rule, keep its positive and negative examples and state the compiler facts it requires,
including their checked phase and snapshot. Missing, pending or stale prerequisites must stay visible;
they cannot produce a success result. Record what source, contract or target changes invalidate the result.
[The documentation index](docs/index.md) maps the existing families and retains the historical F# workflow.
Acceptance requires the server to reproduce the compiler's result and source range for the same document
version, update it after an unsaved edit, and avoid
duplicate analyzer diagnostics. CLR-specific assumptions and suggested source rewrites need reassessment
against Clef semantics; a rename does not establish their applicability.

The [Clef specification](https://github.com/FidelityFramework/clef-lang-spec) governs semantics; the
[consumer contract](https://github.com/FidelityFramework/clef/blob/main/docs/fidelity/phg/Lattice_Consumer_Contract.md)
records compiler and consumer responsibilities. The server implements diagnostics, dimensional hover,
resolved-reference definition and versioned source-proof dispatch. This does not provide a general analyzer
plugin API or establish native lowering proofs.

The [Option waypoint](docs/option-waypoint.md) ties this repository's rule corpus to the current
CCS projection, LSP and native execution gates. Run the inherited analyzer suite with
`dotnet test tests/Lattice.Analyzers.Tests/Lattice.Analyzers.Tests.fsproj -c Release`.
The separate [CCS integration gate](tests/Lattice.CCS.Integration/README.md) consumes compiler-owned
snapshots; it neither ports an FCS rule nor installs that rule into Clef checking.

Upstream license and attribution: see `LICENSE.md`. ionide-analyzers is the work of the Ionide community.
