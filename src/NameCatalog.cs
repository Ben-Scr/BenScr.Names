using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace BenScr.Names;

/// <summary>An immutable, copied pair of first- and last-name lists. Entries and duplicate weights retain input order.</summary>
/// <remarks>Catalogs may be shared between threads. Callers must not mutate their input lists during construction.
/// The catalog does not infer culture, gender, nationality or identity from a spelling.</remarks>
public sealed class NameCatalog
{
    /// <summary>The inclusive maximum number of entries in either custom list.</summary>
    public const int MaximumEntriesPerList = 100_000;
    /// <summary>The inclusive maximum UTF-16 length of a custom entry.</summary>
    public const int MaximumEntryLength = 256;
    /// <summary>The inclusive total character budget across both custom lists.</summary>
    public const int MaximumTotalCharacters = 8_388_608;
    private static readonly Lazy<NameCatalog> embedded = new(() => new NameCatalog(
        ReadEmbedded("FirstNames"), ReadEmbedded("LastNames"), false));

    /// <summary>Gets the thread-safe embedded historical catalog: 41,588 first names and 88,799 last names.</summary>
    public static NameCatalog Default => embedded.Value;
    /// <summary>Gets a read-only view of raw first-name entries.</summary>
    public IReadOnlyList<string> FirstNames { get; }
    /// <summary>Gets a read-only view of raw last-name entries.</summary>
    public IReadOnlyList<string> LastNames { get; }

    /// <summary>Copies nonempty lists, preserving duplicate entries as additional selection weight.</summary>
    /// <remarks>Entries must start with a BMP Unicode letter. Later characters may be BMP letters,
    /// combining marks, ASCII apostrophes or hyphens. Whitespace, digits, controls and surrogate code units
    /// are rejected. Limits apply before publication of the catalog; inputs are never modified.</remarks>
    public NameCatalog(IReadOnlyList<string> firstNames, IReadOnlyList<string> lastNames)
        : this(firstNames, lastNames, true) { }

    private NameCatalog(IReadOnlyList<string> firstNames, IReadOnlyList<string> lastNames, bool validateNames)
    {
        int total = 0;
        FirstNames = Array.AsReadOnly(Copy(firstNames, nameof(firstNames), validateNames, ref total));
        LastNames = Array.AsReadOnly(Copy(lastNames, nameof(lastNames), validateNames, ref total));
    }

    private static string[] Copy(IReadOnlyList<string> input, string parameter, bool validateNames, ref int total)
    {
        ArgumentNullException.ThrowIfNull(input, parameter);
        int count = input.Count;
        if (count is < 1 or > MaximumEntriesPerList)
        {
            throw new ArgumentOutOfRangeException(parameter, "Each catalog list must have 1 to 100000 entries.");
        }
        string[] result = new string[count];
        for (int index = 0; index < count; index++)
        {
            string value = input[index];
            if (value is null || value.Length is < 1 or > MaximumEntryLength || (validateNames && !IsCustomName(value)))
            {
                throw new ArgumentException("A name is null, too long, empty or contains unsupported characters.", parameter);
            }
            if (value.Length > MaximumTotalCharacters - total)
            {
                throw new ArgumentException("The catalog exceeds the total character budget.", parameter);
            }
            total += value.Length;
            result[index] = value;
        }
        return result;
    }

    private static bool IsCustomName(string value)
    {
        if (!char.IsLetter(value[0]))
        {
            return false;
        }
        foreach (char character in value)
        {
            UnicodeCategory category = char.GetUnicodeCategory(character);
            if (!char.IsLetter(character) && character is not ('\'' or '-') &&
                category is not (UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark))
            {
                return false;
            }
        }
        return true;
    }

    private static string[] ReadEmbedded(string name)
    {
        using Stream stream = typeof(NameCatalog).Assembly.GetManifestResourceStream($"BenScr.Names.Resources.{name}.txt")
            ?? throw new InvalidOperationException("Missing embedded name list: " + name);
        using var reader = new StreamReader(stream);
        // Retain the original filtering rules and line order for published catalog compatibility.
        return reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(value => value.All(character => char.IsLetter(character) || character is '\'' or '-')).ToArray();
    }
}
