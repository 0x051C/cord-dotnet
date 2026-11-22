using System.Reflection;

namespace Cord.Core;

/// <summary>A leaf of a flattened record: its dotted property path (e.g. <c>Seller.TaxPayerId</c>) and how to read it.</summary>
public sealed record FlatColumn(string Path, Type Type, Func<object, object?> Read);

/// <summary>
/// Flattens nested records into columns, depth first, in property declaration order:
/// <c>Waybill.Seller.Name</c> becomes the column <c>Seller.Name</c>. Strings and value types are leaves,
/// any other property type is walked into. A null nested record reads as nulls in all of its columns.
/// Used to map waybills onto Dataverse columns, and the shape a spreadsheet export needs (header + rows).
/// </summary>
public static class Flat
{
    public static IReadOnlyList<FlatColumn> Columns<T>() => Cache<T>.Columns;

    /// <summary>The values of <paramref name="record"/>, in <see cref="Columns{T}"/> order.</summary>
    public static IEnumerable<object?> Values<T>(T record) where T : notnull =>
        Columns<T>().Select(column => column.Read(record));

    private static class Cache<T>
    {
        public static readonly IReadOnlyList<FlatColumn> Columns = Walk(typeof(T), "", root => root).ToArray();
    }

    private static IEnumerable<FlatColumn> Walk(Type type, string prefix, Func<object, object?> readOwner) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetIndexParameters().Length == 0)
            .OrderBy(property => property.MetadataToken) // declaration order
            .SelectMany(property =>
            {
                var path = prefix + property.Name;
                object? Read(object root) => readOwner(root) is { } owner ? property.GetValue(owner) : null;

                return IsLeaf(property.PropertyType)
                    ? [new FlatColumn(path, property.PropertyType, Read)]
                    : Walk(property.PropertyType, path + ".", Read);
            });

    private static bool IsLeaf(Type type) => type.IsValueType || type == typeof(string);
}
