using System.Text;

namespace Cord.Core;

/// <summary>Validates an uploaded <c>ttn_*.csv</c> file and saves its valid rows.</summary>
public sealed class WaybillImport(IWaybillStore store)
{
    public async Task<ValidationResult> ImportAsync(string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var result = WaybillValidation.Validate(new SourceFile(fileName, DelimitedFile.ReadRows(reader)));

        if (result.Waybills.Count > 0)
        {
            await store.UpsertAsync(result.Waybills, cancellationToken);
        }
        return result;
    }
}
