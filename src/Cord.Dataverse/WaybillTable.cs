using Cord.Core;
using Microsoft.Xrm.Sdk;

namespace Cord.Dataverse;

/// <summary>
/// The <c>cord_waybill</c> Dataverse table: one column per flattened <see cref="Waybill"/> property,
/// e.g. <c>Seller.TaxPayerId</c> is stored in <c>cord_sellertaxpayerid</c>.
/// </summary>
public static class WaybillTable
{
    public const string LogicalName = "cord_waybill";
    public const string Prefix = "cord_";

    /// <summary>Column of the alternate key that upserts match on.</summary>
    public static readonly string KeyColumn = ColumnName(nameof(Waybill.GlobalUniqueIdentifier));

    /// <summary>Dataverse column logical names with the waybill property each one is read from.</summary>
    public static IReadOnlyList<(string Column, FlatColumn Source)> Columns { get; } =
        Flat.Columns<Waybill>().Select(source => (ColumnName(source.Path), source)).ToArray();

    /// <summary><c>Seller.TaxPayerId</c> -> <c>cord_sellertaxpayerid</c>.</summary>
    public static string ColumnName(string path) =>
        Prefix + path.Replace(".", "", StringComparison.Ordinal).ToLowerInvariant();

    /// <summary>
    /// An entity addressed by its alternate key, so it can be upserted. The key is not repeated in
    /// <see cref="Entity.Attributes"/>: on create Dataverse copies it from <see cref="Entity.KeyAttributes"/>.
    /// </summary>
    public static Entity ToEntity(Waybill waybill)
    {
        var entity = new Entity(LogicalName, KeyColumn, waybill.GlobalUniqueIdentifier);
        foreach (var (column, source) in Columns.Where(c => c.Column != KeyColumn))
        {
            entity[column] = ToDataverseValue(source.Read(waybill));
        }
        return entity;
    }

    // Date only columns are set with a DateTime; text and decimal values are sent as they are.
    private static object? ToDataverseValue(object? value) => value switch
    {
        DateOnly date => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
        "" => null,
        _ => value,
    };
}
