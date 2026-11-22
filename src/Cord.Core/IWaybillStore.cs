namespace Cord.Core;

/// <summary>Where valid waybills are saved. Saving is an upsert on <see cref="Waybill.GlobalUniqueIdentifier"/>,
/// so importing the same file twice does not create duplicates.</summary>
public interface IWaybillStore
{
    Task UpsertAsync(IReadOnlyCollection<Waybill> waybills, CancellationToken cancellationToken = default);
}
