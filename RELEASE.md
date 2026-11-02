# Release verification

Use .NET SDK 10.0.303, .NET 9/10 runtimes, PowerShell 7 and Git. From a clean checkout run:

```powershell
pwsh -File tools/Verify.ps1
```

The verifier restores locked dependencies, builds and checks formatting, runs both framework test
suites and examples, compares the public API, packs matching nupkg/snupkg files, audits metadata,
list-resource hashes, original icon/license, portable PDB identities and Source Link checksums,
then executes external consumers with empty package and HTTP caches. CI repeats this on Windows
and Ubuntu. `artifacts/latest-verification.txt` identifies the exact output directory.

Publish only that verified artifact pair after reviewing the successful CI and exact source commit.
The tag `packages/BenScr.Names/1.0.0`, manifest and nuspec must identify the same immutable commit.
Never move an existing release tag or replace a published version. Tags, configured CI and successful
packing alone do not establish publication.

After NuGet indexing:

```powershell
pwsh -File tools/VerifyConsumer.ps1 -ArtifactDirectory /absolute/path/to/verified/artifacts -Published
```

This restores from the public feed with empty caches, executes both frameworks and compares installed
payloads with the verified package. NuGet signing can change the outer archive checksum. Verify the
repository signature, public symbols and live Source Link files separately; inspect the gallery icon.
Attach the original pair and a final release manifest with actual status and CI URL to the repository
release. Do not put local machine paths or credentials into public release records.

Windows and Ubuntu x64 are the executed support targets. Native AOT, trimming, other architectures
and interactive debugger stepping are not advertised as verified.
