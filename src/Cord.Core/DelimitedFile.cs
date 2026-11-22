namespace Cord.Core;

/// <summary>Reads the <c>^</c>-delimited export files.</summary>
public static class DelimitedFile
{
    public const char DefaultDelimiter = '^';

    /// <summary>
    /// The data rows of a file: the first line is a header and is skipped, blank lines are skipped.
    /// Values are kept verbatim: the files use no quoting, quotes belong to names such as <c>LLC "EXAMPLE"</c>.
    /// </summary>
    public static IEnumerable<string[]> ReadRows(TextReader reader, char delimiter = DefaultDelimiter)
    {
        if (reader.ReadLine() is null)
        {
            yield break; // empty file, not even a header
        }

        while (reader.ReadLine() is { } line)
        {
            var values = line.Split(delimiter);
            if (values.Any(value => !string.IsNullOrWhiteSpace(value)))
            {
                yield return values;
            }
        }
    }
}
