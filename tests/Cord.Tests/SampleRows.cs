namespace Cord.Tests;

/// <summary>A valid source row (the first row of Fixtures/ttn_mock.csv) to derive test rows from.</summary>
internal static class SampleRows
{
    public const int Columns = 27;

    public static string[] Valid() =>
    [
        "MOCK-92762", "09.07.2026", "Экспорт", "14.07.2026", "31/26 of 21.12.2025", "11 of 09.06.2026",
        "LLC \"SPECIMEN CHEMICALS\"", "000404, Demo City, Test Lane, bld. 87", "TEST73140573",
        "TEST104431", "SE \"SAMPLE LOGISTICS\"", "TEST357795", "000303, Exampleburg, Test Lane, bld. 72",
        "SP TESTOV IVAN IVANOVICH", "TEST25941767", "000202, Sampleton, Placeholder Sq., bld. 2",
        "TEST614943", "SP TESTOV IVAN IVANOVICH", "TEST162452", "000606, Placeholder Bay, Test Lane, bld. 14",
        "EUR", "Nitrogen fertilisers", "3102101000", "1897498.37", "1897498.37", "",
        "D4D62887D67B6ABC5E88DF9BEB7249B28D17219C22E75C2C5E57B3DC3AF01593",
    ];

    /// <summary>The valid row with column <paramref name="index"/> (0-based) replaced.</summary>
    public static string[] With(int index, string value)
    {
        var row = Valid();
        row[index] = value;
        return row;
    }

    public static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);
}
