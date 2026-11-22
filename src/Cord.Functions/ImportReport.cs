using System.Text.Encodings.Web;
using System.Text.Json;
using Cord.Core;

namespace Cord.Functions;

/// <summary>The report written next to each imported file, e.g. <c>reports/ttn_2026_11.json</c>.</summary>
public static class ImportReport
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        // Source data is largely Cyrillic: keep it readable instead of \u-escaped.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string ToJson(string fileName, ValidationResult result) =>
        JsonSerializer.Serialize(new
        {
            File = fileName,
            result.Rows,
            Imported = result.Waybills.Count,
            Rejected = result.InvalidRows,
            Errors = result.Errors.Select(error => new { error.Row, error.Field, error.Message, error.Value }),
        }, Options);
}
