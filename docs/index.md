# Lattice analyzer migration

This fork preserves twelve upstream rules and their tests. The next step is to assign their useful behavior
to the CCS/Lattice boundary described in the
[shared integration plan](https://github.com/FidelityFramework/Composer/blob/main/docs/Lattice_Integration.md).
The implemented server is .NET-hosted in Composer's solution; semantic facts come from CCS. That host choice
does not introduce an alternative checker or weaken the native Clef compilation contract.

## Concrete analysis slots

### Target-aware compiler obligations — planned 2026-09-20

[Composer M-01 §5](../../Composer/docs/PRDs/M-01-DialectAdmission.md#5-numeric-selection-parallelism-and-design-time-projection)
maps Numeric Selection, RPC wait classification and Scheduler Contract to
compiler and editor acceptance. Add regression purposes for unsupported numeric
operations/modes, invalid reassociation, partial-sum overflow despite a fitting
final result, unresolved wait order and unavailable supervision recovery.
Required checks belong in Baker/CCS with graph participants and target premise
provenance. A may-wait cycle without feasibility evidence is not a proved
deadlock; an unresolved numeric obligation is not a proved counterexample.

The shared projection must retain the selected target, evidence status and
dependency invalidation. Optional advice cannot weaken representation selection,
arithmetic guarantees or scheduler requirements for speed. Test exact compiler
codes/spans and unsaved repair through the active Lattice service; do not add
semantic inference to the inherited analyzer SDK. These are planned cases,
with implementation evidence recorded in the
[waypoints](../../Composer/docs/Language_Coverage_Waypoints.md).

### Existing migration rules

This repository records useful rule purposes, their prerequisites and regression cases. The inherited FCS
SDK execution path is retired for Clef integration; the repository and the ambition for proof-aware analysis
remain useful. Type resolution and Baker carry and settle information before a consumer asks to display it.
Place each selected rule using the shared plan's
[analysis and analyzer slots](https://github.com/FidelityFramework/Composer/blob/main/docs/Lattice_Integration.md#analysis-and-analyzer-slots):

| Purpose | Execution and result |
|---|---|
| Enforce type correctness or a required safety/proof contract | The required CCS path owns the check and PSG obligations. Removing an optional analyzer must never make an ill-typed program or unmet required proof compile. |
| Explain a settled type, range, layout, premise or proof status | Query compiler-owned facts and project them through LSP, preserving their scope and provenance. |
| Suggest a style change, optimization candidate or additional property | Label it advisory. Any semantic prerequisites come from CCS; a suggestion is not proof that a rewrite is valid or a property holds. A property explicitly accepted as a contract becomes a PSG obligation on the required compiler path. |

For each concrete slot, record the input facts and phase it requires, the checked source/graph snapshot,
and relevant contract or target assumptions. Include missing, pending and stale states, and identify the
dependencies whose changes invalidate a result. Missing prerequisites must not yield success or an
unconditional rewrite; an unresolved required contract stays on the required check path. A solver dispatch
state, its verdict and the premises under which that verdict applies are distinct facts.

Keep the inventory tied to selected rules and tests. It does not define a new source API or require a general
plugin framework before a concrete execution need is established.

## Rule responsibilities

These are migration classifications, not claims that replacement Clef diagnostics already exist.

| Existing rules | First responsibility |
|---|---|
| Record updates ([001](suggestion/001.md)), ignored functions ([003](suggestion/003.md)), option handling ([006](suggestion/006.md)) | Identify the applicable CCS semantic check or query. Preserve types, effects, and source ranges when considering a diagnostic or fix. |
| List patterns ([007](suggestion/007.md)), piped collection functions ([010](performance/010.md)) | Establish native operation semantics and any transformation preconditions in CCS before offering a rewrite. |
| Partial active-pattern layout ([009](performance/009.md)), union layout ([012](suggestion/012.md)) | Query the compiler's settled layout/residence facts; review the upstream recommendation against Clef's representation model. |
| Generic spelling ([002](style/002.md)), union-field names ([004](suggestion/004.md)) | Review as presentation or syntactic style candidates. Any semantic information needed comes from CCS. |
| Empty-string performance ([005](suggestion/005.md)), list equality performance ([008](performance/008.md)), null comparison ([011](performance/011.md)) | Reassess the CLR-specific assumptions. Retain the examples as heritage; only a supported native equivalent should become a compiler check or query. |

The [current project](../src/Lattice.Analyzers/Lattice.Analyzers.fsproj) still references the FSharp.Analyzers
SDK. [Existing tests](../tests/Lattice.Analyzers.Tests) exercise that typed-tree implementation. They provide
candidate cases, not evidence that the same advice is valid for Clef.

[The Option waypoint](option-waypoint.md) is a concrete first integration gate: retain the inherited rule's
resolved-symbol regressions, consume CCS's current Option diagnostics and immutable snapshots, and pair
those results with the live LSP and native runtime gates in their owning repositories.

## First contribution and gate

Choose a rule with a confirmed Clef use case and assign its slot and required compiler facts. Carry that
rule's positive and negative examples into a CCS check/query test, including missing prerequisites and
invalidation after a relevant edit. For mandatory checks, confirm the compiler enforces them without any
optional analyzer enabled. For an advisory property, distinguish the candidate from an accepted contract
and test that acceptance creates a required obligation rather than an assumed result.

Then verify LSP presentation against the same checked source version. Edit the unsaved document and confirm
the result updates or clears at the right range; delayed results must not appear current. Semantic
diagnostics retain the compiler's code, message, severity, and evidence; the editor adds presentation rather
than a second decision procedure.

The current CCS.Editor and Lattice.Server supply the initial snapshot and two-file project boundary. This does
not require migrating all twelve rules or defining a new plugin system before basic editor checking works.
Obligation data and a solver verdict are separate results; display each only when the compiler service
supplies it.

## Target-specific analysis

HelloArty provides an existing compiler-hosted example. CCS's [FPGA depth analysis](https://github.com/FidelityFramework/clef/blob/main/src/Compiler/PSGSaturation/SemanticGraph/DepthAnalysis.fs) selects FPGA contexts and reports `CCS0100` as a warning through the ordinary diagnostic result. It estimates weighted operation depth using clock and calibration inputs, with a heuristic fallback. The editor can expose that result and its contributing operations; it is not a proof of post-route timing closure.

This distinguishes three useful purposes: target-specific advice, required target contracts such as layout or capacity, and results from validating a particular emitted artifact. Their evidence and enforcement differ. Changing the target or its declarations invalidates dependent findings. Making a warning fatal does not strengthen its justification. See the [target-context design](https://github.com/FidelityFramework/Composer/blob/main/docs/Lattice_Integration.md#target-context-selects-the-applicable-analysis).

## Historical F# analyzer workflow

The following upstream instructions are retained for the inherited F# tools and examples. They are not
installation instructions for Lattice.Server.

### Quick start

In its simplest form, running analyzers requires running the [fsharp-analyzers tool](https://www.nuget.org/packages/fsharp-analyzers) and pass your `*.fsproj` and a folder that contains analyzers binaries.

```shell
# Create a new manifest if you don't have one
dotnet new tool-manifest

# Install the local tool
dotnet tool install fsharp-analyzers

# Execute the tool, run dotnet fsharp-analyzers --help for more options. 
dotnet fsharp-analyzers --project ./src/MyProject.fsproj --analyzers-path /var/some-folder
```

Of course, the `--analyzers-path` is a bit tricky. We need to download the binary and find them somehow.

You can add a NuGet reference to your project:

```xml
<PackageReference Include="Ionide.Analyzers" Version="0.1.1">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
</PackageReference>
```

This will download the package to our local NuGet cache.  
*But how do we find that local path?*

Starting `dotnet 8 RC 2`, we can [evaluate an MSBuild property](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8-rc2/#msbuild-simple-cli-based-project-evaluation) which NuGet restore creates for us.

> dotnet build ./src/MyProject.fsproj --getProperty:PkgIonide_Analyzers

`PkgIonide_Analyzers` comes from the [{projectName}.projectFileExtension.nuget.g.props](https://learn.microsoft.com/en-us/nuget/reference/msbuild-targets#restore-outputs) file.

This can yield

> C:\Users\username\\.nuget\packages\ionide.analyzers\0.1.1

And we can use that path to pass to the tool:

> dotnet fsharp-analyzers --project ./src/MyProject.fsproj --analyzers-path C:\Users\username\\.nuget\packages\ionide.analyzers\0.1.1

### Upstream contribution guide

Learn how to [get started contributing]({{fsdocs-next-page-link}})!
