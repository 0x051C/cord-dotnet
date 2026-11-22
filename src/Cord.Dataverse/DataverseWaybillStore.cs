using Cord.Core;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Cord.Dataverse;

/// <summary>Upserts waybills into <see cref="WaybillTable"/> with the <c>UpsertMultiple</c> bulk message.</summary>
public sealed class DataverseWaybillStore(IOrganizationServiceAsync2 service) : IWaybillStore
{
    /// <summary>Microsoft recommends 100 to 1,000 records per request for standard tables.</summary>
    public const int BatchSize = 500;

    public async Task UpsertAsync(IReadOnlyCollection<Waybill> waybills, CancellationToken cancellationToken = default)
    {
        foreach (var batch in Batches(waybills))
        {
            var request = new UpsertMultipleRequest
            {
                Targets = new EntityCollection(batch.Select(WaybillTable.ToEntity).ToList())
                {
                    EntityName = WaybillTable.LogicalName,
                },
            };
            // On a standard table any failing record rolls the whole request back; the upsert is
            // idempotent, so retrying the import (the Functions runtime does) is safe.
            await service.ExecuteAsync(request, cancellationToken);
        }
    }

    /// <summary>
    /// <c>UpsertMultiple</c> rejects a request that holds the same key twice, so a waybill repeated in
    /// a file is sent once, as its last occurrence: the same result as upserting the rows one by one.
    /// </summary>
    public static IEnumerable<Waybill[]> Batches(IEnumerable<Waybill> waybills) =>
        waybills
            .GroupBy(waybill => waybill.GlobalUniqueIdentifier, StringComparer.Ordinal)
            .Select(duplicates => duplicates.Last())
            .Chunk(BatchSize);
}
