# Provenance

The public [RandomNameGenerator repository](https://github.com/Ben-Scr/RandomNameGenerator) is preserved
in this repository's history. Migration source: `master`, commit
`33c2cf9e4cdb57cb68628d57a1d2095d757d818e`. The original local checkout is unchanged.
The published [RandomNameGeneratorCS 1.0.7 package](https://www.nuget.org/packages/RandomNameGeneratorCS/1.0.7)
contains one public type and the eight constructor/member entries mapped in `MIGRATION.md`.
Its only supported framework was .NET 9; .NET 10 is added here. Neither package has runtime dependencies.

Both list resources are copied byte-for-byte from the published assembly. The source checkout used
CRLF for the last-name text while the released resource used LF; the released resource is authoritative
for byte preservation. No entries, ordering or duplicate weights were changed. The historical filter
retains nonempty lines consisting only of BMP letters, ASCII apostrophes and hyphens.

| Published resource | Bytes | SHA-256 |
| --- | ---: | --- |
| `FirstNames.txt` | 359457 | `44842af17b319249cbde8589e17aad115dd02dec8e255536fc9a1813788ba329` |
| `LastNames.txt` | 695422 | `a39e331fed8145943b9cb34b04210fa1fb548068a5fb287c1c7c0cd1708969b6` |

There are 41,588 accepted first-name entries and 88,799 last-name entries. The longest entries contain
22 and 13 characters respectively. Tests compare the complete accepted arrays with hashes captured
from the old public assembly, not just their counts. The original repository supplies these lists under
its included license. No separate upstream list source or culture/gender metadata was identified;
the lists are not presented as linguistically curated or representative of a population.

`LICENSE.txt` is unchanged from that package (SHA-256
`f6600e285e17c48188a65745d7595807e1755c72201e8eef201a9174fca18573`). It is the author's custom license,
including visible attribution requirements. It must not be described as MIT.

The unchanged supplied BenScr brand asset is `branding/BenScr_Icon_Master.png`: transparent RGBA PNG,
1254×1254, 795819 bytes, SHA-256
`a5dcabceb766e13c1840a95130ee98a458e12e3713fd9b07f3ceae75a1f5b993`. Those exact bytes are embedded as
`icon.png`; historical icons are not packaged.

Seeded selection is a local implementation of the documented SplitMix64 integer mixing equations and
rejection-based range mapping. Fixed vectors were independently evaluated with arbitrary-precision
integer arithmetic. The library is not a cryptographic random source. No other library source is used.
