using Cord.Core;

namespace Cord.Tests;

public class WaybillParserTests
{
    [Fact]
    public void Maps_every_column_including_nested_parties_and_item()
    {
        var result = WaybillParser.Parse(SampleRows.Valid());

        Assert.True(result.IsValid);
        var waybill = result.Waybill!;
        Assert.Equal("MOCK-92762", waybill.Number);
        Assert.Equal(new DateOnly(2026, 7, 9), waybill.Registration);
        Assert.Equal(new DateOnly(2026, 7, 14), waybill.CrossedAt);
        Assert.Equal(new Party("LLC \"SPECIMEN CHEMICALS\"", "000404, Demo City, Test Lane, bld. 87", "TEST73140573"), waybill.Seller);
        // The buyer's columns are ordered name, tax payer id, address.
        Assert.Equal(new Party("SP TESTOV IVAN IVANOVICH", "000202, Sampleton, Placeholder Sq., bld. 2", "TEST25941767"), waybill.Buyer);
        Assert.Equal(new WaybillItem("3102101000", 1897498.37m), waybill.Item);
        Assert.Null(waybill.CorrectionDate);
        Assert.Equal("D4D62887D67B6ABC5E88DF9BEB7249B28D17219C22E75C2C5E57B3DC3AF01593", waybill.GlobalUniqueIdentifier);
    }

    [Theory]
    [InlineData("32.13.2022")] // the broken value the Python mock generator uses
    [InlineData("31.02.2026")]
    [InlineData("2026-07-09")]
    public void Rejects_an_invalid_registration_date(string value)
    {
        var result = WaybillParser.Parse(SampleRows.With(1, value));

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("registration", error.Field);
        Assert.Equal(value, error.Value);
    }

    [Fact]
    public void Registration_date_is_required_but_other_dates_may_be_blank()
    {
        Assert.Equal("registration", Assert.Single(WaybillParser.Parse(SampleRows.With(1, "")).Errors).Field);

        var result = WaybillParser.Parse(SampleRows.With(3, ""));
        Assert.True(result.IsValid);
        Assert.Null(result.Waybill!.CrossedAt);
    }

    [Fact]
    public void Rejects_a_feacn_code_longer_than_ten_digits()
    {
        var error = Assert.Single(WaybillParser.Parse(SampleRows.With(22, "31021010001")).Errors);

        Assert.Equal("item.feacn_code", error.Field);
    }

    [Theory]
    [InlineData("1897498,37", 1897498.37)]
    [InlineData(" 1897498.37 ", 1897498.37)]
    [InlineData("100", 100)]
    public void Prices_accept_a_decimal_point_or_comma(string value, double expected)
    {
        var result = WaybillParser.Parse(SampleRows.With(23, value));

        Assert.Equal((decimal)expected, result.Waybill!.Item.Price);
    }

    [Fact]
    public void Rejects_a_price_that_is_not_a_number()
    {
        var error = Assert.Single(WaybillParser.Parse(SampleRows.With(24, "n/a")).Errors);

        Assert.Equal("price_in_national_currency", error.Field);
    }

    [Fact]
    public void Requires_the_unique_identifier_used_as_the_upsert_key()
    {
        var error = Assert.Single(WaybillParser.Parse(SampleRows.With(26, "  ")).Errors);

        Assert.Equal("global_unique_identifier", error.Field);
    }

    [Fact]
    public void A_short_row_reports_the_missing_required_fields()
    {
        var row = SampleRows.Valid()[..20]; // ends after receiver_cargo_handling_terminal

        var result = WaybillParser.Parse(row);

        Assert.False(result.IsValid);
        Assert.Equal(["currency", "item_name", "global_unique_identifier"], result.Errors.Select(e => e.Field));
        Assert.All(result.Errors, error => Assert.Equal("Field required", error.Message));
    }

    [Fact]
    public void Extra_trailing_values_are_ignored()
    {
        Assert.True(WaybillParser.Parse([.. SampleRows.Valid(), "unexpected"]).IsValid);
    }
}
