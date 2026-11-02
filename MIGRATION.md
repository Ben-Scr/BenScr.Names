# Migration from RandomNameGeneratorCS

Replace `RandomNameGeneratorCS 1.0.7` with `BenScr.Names 1.0.0` and change
`using BenScr.RandomNameGenerator;` to `using BenScr.Names;`. Both .NET 9 and .NET 10 are supported.
This is a new package and namespace; there is no binary compatibility with the old assembly.

| Previous public member | New member and behavior |
| --- | --- |
| `NameGenerator()` | Retained; owns a non-cryptographic random stream seeded from the platform RNG instead of using `Random.Shared`. |
| `FirstName()` | Retained; samples one embedded first-name entry and applies historical invariant initial capitalization. |
| `LastName()` | Retained, with the same spelling/casing policy for the last-name list. |
| `FullName(int names = 1)` | Retained; joins first names and a surname with single spaces. The accepted count is now 1–32 rather than any positive integer. Invalid counts do not consume state. |
| `FirstNamesCount()` | Retained; 41,588 embedded entries. Always refers to the default catalog, including on instances configured with a custom catalog. |
| `LastNamesCount()` | Retained; 88,799 embedded entries. |
| `GetFirstNames()` | Retained; returns a new array containing raw embedded spellings. |
| `GetLastNames()` | Retained; returns a new array containing raw embedded spellings. |

New constructors accept an explicit seed and/or `NameCatalog`. `NameCasing.Preserve` returns spelling
unchanged; `InitialCapital` keeps the historical first-character uppercase/rest lowercase behavior.
Custom catalogs preserve duplicate entries and order; each entry has equal selection weight, so repeated
spellings receive more weight. A catalog can be shared between generators, while each generator requires
external synchronization. Inspect `catalog.FirstNames`/`LastNames` for custom counts and spellings.

The exact published list bytes, accepted sequence and duplicates are preserved. The data have no
verified culture or gender annotations. No new taxonomy is inferred. The original API had no seeded
sequence promise; the new version explicitly defines its algorithm. See the provenance and README.
