namespace Cord.Core;

/// <summary>An input file: its name and its data rows.</summary>
public sealed record SourceFile(string Name, IEnumerable<string[]> Rows);

/// <summary>One rejected value, located in its file. <see cref="Row"/> is the 1-based data row (the header is not counted).</summary>
public sealed record RowError(string File, int Row, string Field, string Message, string? Value);

public sealed record ValidationResult(int Rows, IReadOnlyList<Waybill> Waybills, IReadOnlyList<RowError> Errors)
{
    /// <summary>Rows with at least one error (a row can have several).</summary>
    public int InvalidRows => Errors.Select(error => (error.File, error.Row)).Distinct().Count();
}

public static class WaybillValidation
{
    /// <summary>
    /// Validates any number of files as a single stream of rows: <c>SelectMany</c> flattens
    /// files into rows (each tagged with its file and row number), and the per-row errors into one list.
    /// </summary>
    public static ValidationResult Validate(IEnumerable<SourceFile> files)
    {
        var parsed = files
            .SelectMany(file => file.Rows.Select((values, index) => (File: file.Name, Row: index + 1, Values: values)))
            .Select(row => (row.File, row.Row, Result: WaybillParser.Parse(row.Values)))
            .ToList();

        var waybills = parsed
            .Select(row => row.Result.Waybill)
            .OfType<Waybill>()
            .ToList();

        var errors = parsed
            .SelectMany(row => row.Result.Errors.Select(error =>
                new RowError(row.File, row.Row, error.Field, error.Message, error.Value)))
            .ToList();

        return new ValidationResult(parsed.Count, waybills, errors);
    }

    public static ValidationResult Validate(SourceFile file) => Validate([file]);
}
