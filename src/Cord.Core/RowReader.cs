using System.Globalization;

namespace Cord.Core;

/// <summary>A problem with one value of a row. <see cref="Field"/> is the source field path, e.g. <c>item.price</c>.</summary>
public sealed record FieldError(string Field, string Message, string? Value);

/// <summary>
/// Consumes the values of one source row left to right, converting them and collecting <see cref="FieldError"/>s.
/// When a row is too short, the missing required fields are reported and the missing optional ones get defaults;
/// extra trailing values are ignored.
/// </summary>
internal sealed class RowReader(IReadOnlyList<string> values)
{
    private const string DateFormat = "dd.MM.yyyy";
    private int _next;

    public List<FieldError> Errors { get; } = [];

    /// <summary>A text value; <paramref name="required"/> means the column must be present (it may be empty).</summary>
    public string Text(string field, bool required = false, int? maxLength = null)
    {
        var value = Take(field, required) ?? "";
        if (value.Length > maxLength)
        {
            Fail(field, $"String should have at most {maxLength} characters", value);
        }
        return value;
    }

    /// <summary>An optional text value, empty means <c>null</c>.</summary>
    public string? TextOrNull(string field) => Take(field, required: false) is { Length: > 0 } value ? value : null;

    /// <summary>A text value that must be present and not blank (identifiers, keys).</summary>
    public string Key(string field)
    {
        var value = Take(field, required: true);
        if (value is not null && string.IsNullOrWhiteSpace(value))
        {
            Fail(field, "Value must not be blank", value);
        }
        return value ?? "";
    }

    /// <summary>A <c>dd.MM.yyyy</c> date; <paramref name="required"/> means it must be present and not blank.</summary>
    public DateOnly? Date(string field, bool required = false)
    {
        var text = Take(field, required)?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            if (required && text is not null)
            {
                Fail(field, "Date required", text);
            }
            return null;
        }
        if (DateOnly.TryParseExact(text, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        Fail(field, $"Not a valid {DateFormat} date", text);
        return null;
    }

    /// <summary>An optional decimal number with a decimal point or comma; blank means <c>null</c>.</summary>
    public decimal? Decimal(string field)
    {
        var text = Take(field, required: false)?.Trim();
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        if (decimal.TryParse(text.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }
        Fail(field, "Not a number", text);
        return null;
    }

    /// <summary>The next value, or <c>null</c> (reported when required) once the row has run out of values.</summary>
    private string? Take(string field, bool required)
    {
        if (_next < values.Count)
        {
            return values[_next++];
        }
        _next++;
        if (required)
        {
            Fail(field, "Field required");
        }
        return null;
    }

    private void Fail(string field, string message, string? value = null) => Errors.Add(new(field, message, value));
}
