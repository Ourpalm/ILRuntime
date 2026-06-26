---
name: "ilspy-decompile"
description: "Decompile .NET assemblies using ilspycmd CLI tool. Invoke when you need to inspect compiled DLLs, compare original vs patched assemblies, or examine compiler-generated types (closures, iterators, state machines)."
---

# ILSpy Command-Line Decompilation

Decompile .NET assemblies using `ilspycmd` to inspect compiled code structure, compare builds, and debug HybridPatch issues.

## Prerequisites

`ilspycmd` is installed as a global .NET tool:
```
dotnet tool install --global ilspycmd
```

## Common Commands

### List all types in an assembly
```
ilspycmd -l MyLibrary.dll
```
Use this first to discover type names, especially compiler-generated ones.

### Decompile a specific type
```
ilspycmd -t "Namespace.ClassName" MyLibrary.dll
```
For nested types, use `/` as separator: `ilspycmd -t "Outer/Inner" MyLibrary.dll`

### Decompile entire assembly to stdout
```
ilspycmd MyLibrary.dll
```

### Decompile to a project folder
```
ilspycmd MyLibrary.dll -p -o ./OutputDir
```

### Decompile to a single file
```
ilspycmd MyLibrary.dll > Output.cs
```

## Key Options

| Option | Description |
|--------|-------------|
| `-o, --output <dir>` | Set output directory or file path |
| `-p, --project` | Export as C# Visual Studio project |
| `-t, --type <name>` | Decompile only the specified type |
| `-l` | List types without full decompilation |
| `--no-dead-code` | Remove redundant compiler-generated blocks |

## Typical Workflow for HybridPatch Debugging

1. **List types** in both original and patched DLLs to spot naming differences:
   ```
   ilspycmd -l HotfixAOT/bin/Release/net8.0/HotfixAOT.dll
   ilspycmd -l HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll
   ```

2. **Compare compiler-generated types** (closures `<>c__DisplayClass*`, state machines `<Method>d__*`):
   ```
   ilspycmd -t "HotfixAOT.SomeClass/<>c__DisplayClass0_0" original.dll
   ilspycmd -t "HotfixAOT.SomeClass/<>c__DisplayClass0_0" patched.dll
   ```

3. **Inspect injected DLL** to verify injection results:
   ```
   ilspycmd -t "HotfixAOT.SomeClass" HotfixAOT/Patched/HotfixAOT.dll
   ```

4. **Check hash file** for type name mappings after `FixClosureNameConsistency`:
   - The `.hash` file records type hashes used for closure renaming
   - Compare type names in hash vs actual DLL to find mismatches

## Key Paths in This Project

- Original (Release) DLL: `HotfixAOT/bin/Release/net8.0/HotfixAOT.dll`
- Patched (Release_Patched) DLL: `HotfixAOT/bin/Release_Patched/net8.0/HotfixAOT.dll`
- Injected + Patched DLL: `HotfixAOT/Patched/HotfixAOT.dll`
- Hash file: `HotfixAOT/Patched/HotfixAOT.hash`
- Patch file: `HotfixAOT/Patched/HotfixAOT.patch`
