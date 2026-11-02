namespace BenScr.Names;

/// <summary>Controls formatting without modifying the catalog.</summary>
public enum NameCasing
{
    /// <summary>Returns each entry exactly as supplied, without Unicode normalization.</summary>
    Preserve,
    /// <summary>Uppercases the first UTF-16 character and lowercases the remainder using invariant casing.
    /// This is the historical behavior, not linguistic title casing after apostrophes or hyphens.</summary>
    InitialCapital
}
