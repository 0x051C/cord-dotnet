using Cord.Core;
using Cord.Dataverse;

namespace Cord.Tests;

public class WaybillTableTests
{
    private static Waybill Sample(string guid = "GUID-1", string number = "MOCK-1") =>
        WaybillParser.Parse(SampleRows.Valid()).Waybill! with { GlobalUniqueIdentifier = guid, Number = number };

    [Fact]
    public void Column_names_are_the_prefixed_lower_case_property_paths()
    {
        var columns = WaybillTable.Columns.Select(c => c.Column).ToList();

        Assert.Equal(SampleRows.Columns, columns.Count);
        Assert.Contains("cord_number", columns);
        Assert.Contains("cord_sellertaxpayerid", columns);
        Assert.Contains("cord_itemprice", columns);
        Assert.Equal("cord_globaluniqueidentifier", WaybillTable.KeyColumn);
        Assert.All(columns, column => Assert.True(column.Length <= 50, $"{column} is longer than Dataverse allows"));
    }

    [Fact]
    public void An_entity_is_addressed_by_its_alternate_key()
    {
        var entity = WaybillTable.ToEntity(Sample("D4D6"));

        Assert.Equal("cord_waybill", entity.LogicalName);
        Assert.Equal("D4D6", entity.KeyAttributes["cord_globaluniqueidentifier"]);
        Assert.False(entity.Contains("cord_globaluniqueidentifier"));
        Assert.Equal(SampleRows.Columns - 1, entity.Attributes.Count);
    }

    [Fact]
    public void Dates_become_date_times_and_blank_text_becomes_null()
    {
        var entity = WaybillTable.ToEntity(Sample() with { Contract = "" });

        Assert.Equal(new DateTime(2026, 7, 9), entity["cord_registration"]);
        Assert.Equal(DateTimeKind.Unspecified, ((DateTime)entity["cord_registration"]).Kind);
        Assert.Null(entity["cord_contract"]);
        Assert.Null(entity["cord_correctiondate"]);
        Assert.Equal(1897498.37m, entity["cord_itemprice"]);
    }

    [Fact]
    public void A_repeated_key_is_sent_once_as_its_last_occurrence()
    {
        var batches = DataverseWaybillStore.Batches([Sample("A", "first"), Sample("B"), Sample("A", "second")]).ToList();

        var batch = Assert.Single(batches);
        Assert.Equal(["A", "B"], batch.Select(w => w.GlobalUniqueIdentifier));
        Assert.Equal("second", batch[0].Number);
    }

    [Fact]
    public void Large_imports_are_split_into_batches()
    {
        var waybills = Enumerable.Range(0, 1_201).Select(i => Sample($"G{i}"));

        Assert.Equal([500, 500, 201], DataverseWaybillStore.Batches(waybills).Select(b => b.Length));
    }
}
