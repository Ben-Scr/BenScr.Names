using System;
using BenScr.Names;

var names = new NameGenerator(new NameCatalog(["Ada", "Bea", "Cora"], ["Oak", "Pine"]), 0);
string value = names.FullName(2);
if (value != "Bea Ada Pine")
{
    throw new InvalidOperationException("Seeded example differs: " + value);
}
if (NameGenerator.FirstNamesCount() != 41588 || NameGenerator.LastNamesCount() != 88799)
{
    throw new InvalidOperationException("Embedded catalog counts differ.");
}
var preserve = new NameGenerator(new NameCatalog(["Jean-Luc"], ["O'Neill"]), 3, NameCasing.Preserve);
if (preserve.FullName() != "Jean-Luc O'Neill")
{
    throw new InvalidOperationException("Preserved casing differs.");
}
Console.WriteLine(value);
Console.WriteLine(new NameGenerator(42).FullName());
Console.WriteLine("Seeded selection, embedded catalogs and custom casing passed.");
