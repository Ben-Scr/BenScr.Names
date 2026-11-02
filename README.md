# BenScr.Names

Seeded name generation with immutable custom catalogs for .NET 9 and .NET 10. The package includes
41,588 historical first-name entries and 88,799 last-name entries, with no runtime dependencies.

Created by Ben Schneider — https://github.com/Ben-Scr

```sh
dotnet add package BenScr.Names --version 1.0.0
```

```csharp
using BenScr.Names;

var names = new NameGenerator(seed: 42);
string fullName = names.FullName(2);

var catalog = new NameCatalog(
    ["Ada", "Bea", "Cora"],
    ["Oak", "Pine"]);
var custom = new NameGenerator(catalog, seed: 0);
string result = custom.FullName(2); // Bea Ada Pine

var preserved = new NameGenerator(
    new NameCatalog(["Jean-Luc"], ["O'Neill"]), seed: 3, NameCasing.Preserve);
string exactSpelling = preserved.FullName(); // Jean-Luc O'Neill
```

The [complete example](https://github.com/Ben-Scr/BenScr.Names/blob/master/examples/Program.cs)
checks seeded output, embedded catalogs and casing.

## Contracts

`FirstName()` and `LastName()` draw independently with replacement. Duplicates are allowed, and results
are not unique identifiers or representations of real people. `FullName(n)` draws n first-name parts
followed by a last name, separated by single spaces; n must be 1–32 inclusive. Invalid counts consume
no random state. Calls allocate returned strings and, for full names, a temporary builder.

The default `InitialCapital` casing uppercases the first UTF-16 character and lowercases the rest
using invariant .NET casing. It preserves historical behavior: `O'NEILL` becomes `O'neill`, not a
linguistically inferred spelling. `Preserve` returns entries unchanged, without Unicode normalization.

Custom catalogs copy both input lists. Each must contain 1–100,000 entries, each 1–256 UTF-16 code
units, with a combined budget of 8,388,608 code units. Entries must start with a BMP Unicode letter;
remaining characters may be BMP letters, combining marks, ASCII apostrophes or hyphens. Spaces,
digits, controls and surrogate code units are rejected. This deliberately excludes non-BMP names;
no claim of universal human-name validation is made. Catalogs do not infer culture, gender or origin.
Use application-selected lists to define a category. Ordering and duplicate entries are retained,
so repeating a name increases its selection weight.

`NameCatalog.FirstNames`/`LastNames` are read-only views. Input arrays cannot mutate a catalog after
construction. Catalogs can be shared concurrently, but each generator owns mutable state and needs
external synchronization. Input lists must remain stable while their constructor copies them.

The legacy static `FirstNamesCount()`, `LastNamesCount()`, `GetFirstNames()` and `GetLastNames()` always
refer to the embedded catalog. The getters return fresh arrays of raw spellings. Embedded lists keep
the original filter, ordering and duplicate weights; they have no verified language or gender labels.
See [data provenance](https://github.com/Ben-Scr/BenScr.Names/blob/master/docs/PROVENANCE.md).

## Reproducibility

`AlgorithmId` is `splitmix64-name-selection-v1`. Any `ulong` seed, including zero, is valid. Each raw
step adds `0x9E3779B97F4A7C15` to state, mixes with right shifts 30/27/31 and multipliers
`0xBF58476D1CE4E5B9` and `0x94D049BB133111EB`; arithmetic wraps modulo 2^64. For a list of size b,
discard values smaller than `(2^64 mod b)`, then select `value mod b`. This avoids incomplete residue
groups. Seed zero begins with `e220a8397b1dcdaf`, `6e789e6aa1b965f4`, `06c45d188009454f`.

The same seed, catalog order and call sequence produce the same selection indices. Unicode casing
tables follow the .NET runtime; use `Preserve` for output independent of those casing rules. Catalog
changes can change seeded names and are compatibility changes. Constructors without a seed use eight
bytes from `RandomNumberGenerator` for initialization; subsequent draws are not cryptographic. Use
this library for examples, test data and simulations, not secrets or identity generation.

## Build and verification

Requires .NET SDK 10.0.303, .NET 9/10 runtimes, PowerShell 7 and Git. Versions and restore locks are
committed. Tests use xUnit v3 4.0.2 with Microsoft Testing Platform.

```powershell
dotnet test --project tests/BenScr.Names.Tests.csproj -c Release
pwsh -File tools/Verify.ps1
```

Verification covers fixed integer vectors, rejection mapping, full legacy-list fingerprints, bounds,
immutability, custom spelling, casing, concurrent use of shared catalogs, public API and package
metadata, original assets, portable PDB/Source Link and isolated package consumers on both frameworks.
Windows and Ubuntu x64 CI executes the same workflow. See the
[release procedure](https://github.com/Ben-Scr/BenScr.Names/blob/master/RELEASE.md),
[migration mapping](https://github.com/Ben-Scr/BenScr.Names/blob/master/MIGRATION.md) and
[changelog](https://github.com/Ben-Scr/BenScr.Names/blob/master/CHANGELOG.md).

The original [license](https://github.com/Ben-Scr/BenScr.Names/blob/master/LICENSE.txt) requires visible
author attribution. The original BenScr icon is embedded unchanged.
