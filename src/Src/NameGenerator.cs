using System;
using System.Text;

namespace BenScr.Names;

/// <summary>Generates names by independent draws with replacement from an immutable catalog.</summary>
/// <remarks>Instances own mutable random state and require external synchronization. Names are examples,
/// not unique identifiers or representations of real people. This generator is not cryptographic.</remarks>
public sealed class NameGenerator
{
    /// <summary>Identifies the seeded selection algorithm, including range reduction.</summary>
    public const string AlgorithmId = "splitmix64-name-selection-v1";
    /// <summary>The inclusive maximum number of first-name parts in a full name.</summary>
    public const int MaximumFirstNameParts = 32;
    private readonly NameCatalog catalog;
    private readonly NameCasing casing;
    private readonly SeededIndex random;

    /// <summary>Uses the embedded catalog, initial-capital casing and a platform-random seed.</summary>
    public NameGenerator() : this(NameCatalog.Default, SeededIndex.CreateSeed()) { }
    /// <summary>Uses the embedded catalog and a reproducible seed, including zero.</summary>
    public NameGenerator(ulong seed) : this(NameCatalog.Default, seed) { }
    /// <summary>Uses a custom catalog and a platform-random seed. The immutable catalog is shared.</summary>
    public NameGenerator(NameCatalog catalog)
        : this(catalog, SeededIndex.CreateSeed()) { }
    /// <summary>Uses an immutable catalog, exact seed and casing. Null or undefined casing is rejected.</summary>
    public NameGenerator(NameCatalog catalog, ulong seed, NameCasing casing = NameCasing.InitialCapital)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (!Enum.IsDefined(casing))
        {
            throw new ArgumentOutOfRangeException(nameof(casing));
        }
        this.catalog = catalog;
        this.casing = casing;
        random = new SeededIndex(seed);
    }
    /// <summary>Returns a first-name entry, applying the selected casing.</summary>
    public string FirstName() => Format(catalog.FirstNames[random.Next(catalog.FirstNames.Count)]);
    /// <summary>Returns a last-name entry, applying the selected casing.</summary>
    public string LastName() => Format(catalog.LastNames[random.Next(catalog.LastNames.Count)]);
    /// <summary>Returns one to 32 first-name parts followed by one last name, separated by single spaces.</summary>
    /// <remarks>Draw order is repeated FirstName calls followed by LastName. Invalid counts consume no state.</remarks>
    public string FullName(int names = 1)
    {
        if (names is < 1 or > MaximumFirstNameParts)
        {
            throw new ArgumentOutOfRangeException(nameof(names), names, "The first-name count must be between 1 and 32.");
        }
        var result = new StringBuilder();
        for (int index = 0; index < names; index++)
        {
            result.Append(FirstName()).Append(' ');
        }
        return result.Append(LastName()).ToString();
    }
    /// <summary>Returns the number of first-name entries in the embedded catalog, including duplicates.</summary>
    public static int FirstNamesCount() => NameCatalog.Default.FirstNames.Count;
    /// <summary>Returns the number of last-name entries in the embedded catalog, including duplicates.</summary>
    public static int LastNamesCount() => NameCatalog.Default.LastNames.Count;
    /// <summary>Returns a fresh array of raw embedded first names, unaffected by generator casing.</summary>
    public static string[] GetFirstNames() => Copy(NameCatalog.Default.FirstNames);
    /// <summary>Returns a fresh array of raw embedded last names, unaffected by generator casing.</summary>
    public static string[] GetLastNames() => Copy(NameCatalog.Default.LastNames);
    private string Format(string name) => casing == NameCasing.Preserve
        ? name
        : char.ToUpperInvariant(name[0]) + name[1..].ToLowerInvariant();
    private static string[] Copy(System.Collections.Generic.IReadOnlyList<string> values)
    {
        string[] result = new string[values.Count];
        for (int index = 0; index < result.Length; index++)
        {
            result[index] = values[index];
        }
        return result;
    }
}
