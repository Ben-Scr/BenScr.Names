# Verification record

The migration preserves the public RandomNameGenerator history and all eight old constructor/member
entries. The published 1.0.7 list bytes and custom author license are retained exactly. Local build,
style, tests, API baseline, examples, package/PDB/Source Link audit and isolated package consumers
passed for .NET 9 and .NET 10. Each framework executed 34 tests with none skipped.

Tests include full legacy-list fingerprints, independent integer vectors, forced rejection sampling,
custom catalogs, immutable ownership, Unicode/casing constraints, resource limits and separate seeded
generators sharing one catalog. The seeded-zero regression prevents integer zero from selecting an
unseeded casing overload. The API uses an unseeded one-argument catalog constructor to avoid that
ambiguity.

An initial local MSBuild invocation reported internal CLR error 0x80131506. The unchanged build and
full verification succeeded with build-server/node reuse disabled. No test expectations were changed
to accommodate that tooling failure.

Exact artifact names, source commits, hashes and execution results are recorded by tools/Verify.ps1
in artifacts/verification-*/release-manifest.json. Remote CI and public-feed publication have separate
results; this file does not assert publication or a completed remote workflow.
