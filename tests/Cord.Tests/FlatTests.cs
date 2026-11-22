using Cord.Core;

namespace Cord.Tests;

public class FlatTests
{
    [Fact]
    public void Flattens_nested_records_depth_first_in_declaration_order()
    {
        var paths = Flat.Columns<Waybill>().Select(column => column.Path).ToList();

        Assert.Equal(SampleRows.Columns, paths.Count);
        Assert.Equal(
        [
            "Number", "Registration", "StatedGoal", "CrossedAt", "Contract", "Specification",
            "Seller.Name", "Seller.Address", "Seller.TaxPayerId",
            "SellerBank", "SenderName", "SenderTaxPayerId", "SenderCargoHandlingTerminal",
            "Buyer.Name", "Buyer.Address", "Buyer.TaxPayerId",
            "BuyerBank", "ReceiverName", "ReceiverTaxPayerId", "ReceiverCargoHandlingTerminal",
            "Currency", "ItemName", "Item.FeacnCode", "Item.Price",
            "PriceInNationalCurrency", "CorrectionDate", "GlobalUniqueIdentifier",
        ], paths);
    }

    [Fact]
    public void Values_follow_the_column_order()
    {
        var waybill = WaybillParser.Parse(SampleRows.Valid()).Waybill!;

        var values = Flat.Values(waybill).ToList();

        Assert.Equal("MOCK-92762", values[0]);
        Assert.Equal("TEST73140573", values[8]); // Seller.TaxPayerId
        Assert.Equal(1897498.37m, values[23]); // Item.Price
        Assert.Null(values[25]); // CorrectionDate
    }

    private sealed record Outer(string Name, Inner? Inner);

    private sealed record Inner(int Value);

    [Fact]
    public void A_null_nested_record_reads_as_nulls()
    {
        Assert.Equal(["x", null], Flat.Values(new Outer("x", null)));
        Assert.Equal(["x", 1], Flat.Values(new Outer("x", new Inner(1))));
    }
}
