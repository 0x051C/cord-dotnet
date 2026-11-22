using Cord.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Cord.Functions;

/// <summary>
/// Runs when a file is uploaded to <c>waybills/incoming/</c>: validates it, upserts the valid rows into
/// Dataverse and writes a JSON report with every rejected value to <c>waybills/reports/</c>.
/// </summary>
public sealed class ImportWaybills(WaybillImport import, ILogger<ImportWaybills> logger)
{
    private const string Connection = "CordStorage";

    [Function(nameof(ImportWaybills))]
    [BlobOutput("waybills/reports/{name}.json", Connection = Connection)]
    public async Task<string> RunAsync(
        [BlobTrigger("waybills/incoming/{name}.csv", Connection = Connection)] Stream file,
        string name,
        CancellationToken cancellationToken)
    {
        var fileName = name + ".csv";
        var result = await import.ImportAsync(fileName, file, cancellationToken);

        logger.LogInformation(
            "{File}: {Valid} of {Rows} rows imported, {Invalid} rejected with {Errors} errors",
            fileName, result.Waybills.Count, result.Rows, result.InvalidRows, result.Errors.Count);

        return ImportReport.ToJson(fileName, result);
    }
}
