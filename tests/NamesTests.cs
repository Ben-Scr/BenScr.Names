using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BenScr.Names.Tests;

public sealed class NamesTests
{
    [Fact]
    public void RawAlgorithmMatchesIndependentIntegerVectors()
    {
        ulong[] expected = [0xe220a8397b1dcdafUL, 0x6e789e6aa1b965f4UL, 0x06c45d188009454fUL,
            0xf88bb8a8724c81ecUL, 0x1b39896a51a8749bUL, 0x53cb9f0c747ea2eaUL];
        var random = new SeededIndex(0);
        foreach (ulong value in expected)
        {
            Assert.Equal(value, random.NextUInt64());
        }
    }

    [Fact]
    public void IncompleteResidueGroupIsRejected()
    {
        // This seed produces zero first; bound three has threshold one, so the next output is used.
        var random = new SeededIndex(0x61C8864680B583EBUL);
        Assert.Equal(1, random.Next(3));
        Assert.Equal(0x6e789e6aa1b965f4UL, random.NextUInt64());
    }

    [Fact]
    public void SeededSelectionsAndCallOrderAreFrozen()
    {
        var catalog = new NameCatalog(["Ada", "Bea", "Cora"], ["Oak", "Pine"]);
        var generator = new NameGenerator(catalog, 0);
        Assert.Equal("Bea", generator.FirstName());
        Assert.Equal("Ada", generator.FirstName());
        Assert.Equal("Pine", generator.LastName());
        Assert.Equal("Bea Bea Oak", generator.FullName(2));
    }

    [Fact]
    public void CatalogMatchesPublishedLegacyLists()
    {
        string[] first = NameGenerator.GetFirstNames();
        string[] last = NameGenerator.GetLastNames();
        Assert.Equal(41588, NameGenerator.FirstNamesCount());
        Assert.Equal(88799, NameGenerator.LastNamesCount());
        Assert.Equal("AAFILE", first[0]);
        Assert.Equal("ZYTLER", first[^1]);
        Assert.Equal("SMITH", last[0]);
        Assert.Equal("AALDERINK", last[^1]);
        Assert.Equal("310C5753B470C5BDFC42F4631C653B9013C8B5C9B44C116BE96E9FC6984D0360", Hash(first));
        Assert.Equal("1F11E1C64D93744297DB1B47699763CF4F89D825C85458658D6CEBA7C6DBEEA6", Hash(last));
    }

    [Fact]
    public void InputCopiesAndViewsDoNotPermitMutation()
    {
        string[] first = ["Ada"];
        var last = new List<string> { "Oak" };
        var catalog = new NameCatalog(first, last);
        first[0] = "Changed";
        last.Clear();
        Assert.Equal("Ada Oak", new NameGenerator(catalog, 0).FullName());
        Assert.Throws<NotSupportedException>(() => ((IList<string>)catalog.FirstNames)[0] = "Changed");
        string[] embedded = NameGenerator.GetFirstNames();
        embedded[0] = "Changed";
        Assert.Equal("AAFILE", NameGenerator.GetFirstNames()[0]);
    }

    [Fact]
    public void DuplicateEntriesRetainOrderAndSelectionWeight()
    {
        var catalog = new NameCatalog(["Ada", "Ada", "Bea"], ["Oak"]);
        Assert.Equal(new[] { "Ada", "Ada", "Bea" }, catalog.FirstNames);
        var generator = new NameGenerator(catalog, 0);
        Assert.Equal("Ada", generator.FirstName());
        Assert.Equal("Ada", generator.FirstName());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Ada Lovelace")]
    [InlineData("Anna3")]
    [InlineData("-Anne")]
    [InlineData("'Anne")]
    [InlineData("Anne\n")]
    [InlineData("\u0301Anne")]
    [InlineData("\ud800")]
    [InlineData("Anne\ud800")]
    [InlineData("\U00010400")]
    public void InvalidCustomEntriesAreRejected(string? value)
    {
        Assert.Throws<ArgumentException>(() => new NameCatalog([value!], ["Oak"]));
        Assert.Throws<ArgumentException>(() => new NameCatalog(["Ada"], [value!]));
    }

    [Theory]
    [InlineData("Élodie")]
    [InlineData("E\u0301lodie")]
    [InlineData("O'NEILL")]
    [InlineData("JEAN-LUC")]
    [InlineData("李")]
    public void PreserveCasingKeepsCustomSpelling(string value)
    {
        var generator = new NameGenerator(new NameCatalog([value], [value]), 7, NameCasing.Preserve);
        Assert.Equal(value + " " + value, generator.FullName());
    }

    [Fact]
    public void InitialCapitalIsInvariantAndMatchesHistoricalBehavior()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var generator = new NameGenerator(new NameCatalog(["iVAN"], ["O'NEILL"]), 1);
            Assert.Equal("Ivan O'neill", generator.FullName());
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void EmptyAndNullCatalogArgumentsAreRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new NameCatalog(null!, ["Oak"]));
        Assert.Throws<ArgumentNullException>(() => new NameCatalog(["Ada"], null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NameCatalog([], ["Oak"]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NameCatalog(["Ada"], []));
        Assert.Throws<ArgumentNullException>(() => new NameGenerator(null!, 0UL));
        Assert.Throws<ArgumentOutOfRangeException>(() => new NameGenerator(NameCatalog.Default, 0, (NameCasing)42));
    }

    [Fact]
    public void CatalogEntryAndCountLimitsAreInclusive()
    {
        string maximum = new('A', NameCatalog.MaximumEntryLength);
        var catalog = new NameCatalog([maximum], ["Oak"]);
        Assert.Equal(256, catalog.FirstNames[0].Length);
        Assert.Throws<ArgumentException>(() => new NameCatalog([maximum + "A"], ["Oak"]));
        var maximumCount = Enumerable.Repeat("A", NameCatalog.MaximumEntriesPerList).ToArray();
        Assert.Equal(100000, new NameCatalog(maximumCount, ["B"]).FirstNames.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new NameCatalog(new string[100001], ["B"]));
    }

    [Fact]
    public void TotalCharacterBudgetIsCheckedAcrossBothLists()
    {
        string entry = new('A', 256);
        string[] first = Enumerable.Repeat(entry, 32767).ToArray();
        Assert.Single(new NameCatalog(first, [entry]).LastNames);
        Assert.Throws<ArgumentException>(() => new NameCatalog(first, [entry, "B"]));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(33)]
    [InlineData(int.MaxValue)]
    public void InvalidFullNameCountsDoNotConsumeState(int count)
    {
        var generator = new NameGenerator(23);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.FullName(count));
        Assert.Equal(new NameGenerator(23).FullName(), generator.FullName());
    }

    [Fact]
    public void MaximumFullNameHasExactlyThirtyTwoFirstParts()
    {
        var generator = new NameGenerator(new NameCatalog(["Ada"], ["Oak"]), 0);
        Assert.Equal(string.Join(" ", Enumerable.Repeat("Ada", 32)) + " Oak", generator.FullName(32));
    }

    [Fact]
    public void DefaultAndCustomUnseededGeneratorsProduceEligibleNames()
    {
        Assert.NotEmpty(new NameGenerator().FirstName());
        Assert.Equal("Ada Oak", new NameGenerator(new NameCatalog(["Ada"], ["Oak"])).FullName());
    }

    [Fact]
    public void SharedCatalogSupportsIndependentConcurrentGenerators()
    {
        var catalog = new NameCatalog(["Ada", "Bea", "Cora"], ["Oak", "Pine"]);
        string[] results = new string[64];
        Parallel.For(0, results.Length, index => results[index] = new NameGenerator(catalog, 0).FullName(2));
        Assert.All(results, value => Assert.Equal("Bea Ada Pine", value));
    }

    private static string Hash(string[] values) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", values))));
}
