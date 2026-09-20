# Selected program-lifetime projection gate

This companion to `Lattice.CCS.Integration` checks the actual selected-platform
project path through CCS.Editor. It shares Composer's fixture and projection
assertions: renamed immutable/mutable space roles, exact CCS8206/CCS8207 source
diagnostics, dependency definitions and unsaved repairs. Earlier snapshots and
disk sources must remain unchanged.
The companion also checks source-only startup order, exact initializer identity
and source spans, pending native dependency facts, and repair through the
compiler-owned snapshot projection.

After the coordinated compiler/editor build:

```sh
dotnet run --project tests/Lattice.CCS.ProgramLifetime/Lattice.CCS.ProgramLifetime.fsproj -p:BuildProjectReferences=false
```

The temporary evidence directory records the compiler identity and source
revisions. This checks compiler projections; it adds no analyzer diagnostic,
platform catalogue, native image or proof-discharge claim.
