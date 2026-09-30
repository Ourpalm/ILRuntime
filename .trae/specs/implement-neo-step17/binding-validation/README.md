# Generated Neo binding validation

Run from the repository root after building Debug_Neo CLI and Debug TestCases:

```powershell
dotnet run --project .trae/specs/implement-neo-step17/binding-validation/Validation.csproj -p:GenerateBindings=true -- generate .trae/specs/implement-neo-step17/binding-validation/Generated
dotnet run --project .trae/specs/implement-neo-step17/binding-validation/Validation.csproj -- run TestCases/bin/Debug/netstandard2.1/TestCases.dll
```

The first invocation writes bindings into the dedicated Generated directory. The second compiles those bindings and runs five CLR ref/out scenarios with them registered. Generated output and build artifacts are ignored. The harness references the existing Debug_Neo binaries and does not replace the repository's generated bindings.

`GenerateBindings=true` excludes existing generated source while regenerating, so old API names cannot prevent regeneration.

After building the Release_Neo CLI, run the reference workload timing/allocation check:

```powershell
dotnet run --project .trae/specs/implement-neo-step17/binding-validation/Validation.csproj -c Release -p:RuntimeConfiguration=Release_Neo -- bench TestCases/bin/Debug/netstandard2.1/TestCases.dll
```

This warms up each workload, then reports the median of nine 10,000-invocation samples and allocation per invocation. It does not register generated bindings. It includes invocation, reflection, object creation and GC costs; use it as an end-to-end smoke benchmark, not a precise measurement of branch cost.
