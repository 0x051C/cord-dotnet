namespace Cord.Core;

/// <summary>The outcome of parsing one row: a waybill, or the reasons it was rejected.</summary>
public sealed record ParseResult(Waybill? Waybill, IReadOnlyList<FieldError> Errors)
{
    public bool IsValid => Waybill is not null;
}

/// <summary>Maps a flat <c>ttn_*.csv</c> row onto a <see cref="Waybill"/> and validates it.</summary>
public static class WaybillParser
{
    public const int FeacnLength = 10;

    /// <summary>Parses the 27 source columns, in source order (see the header of a <c>ttn_*.csv</c> file).</summary>
    public static ParseResult Parse(IReadOnlyList<string> values)
    {
        var row = new RowReader(values);

        var waybill = new Waybill
        {
            Number = row.Text("id", required: true),
            Registration = row.Date("registration", required: true) ?? default,
            StatedGoal = row.Text("stated_goal", required: true),
            CrossedAt = row.Date("crossed_at"),
            Contract = row.Text("contract"),
            Specification = row.Text("specification"),
            Seller = new Party(
                Name: row.Text("seller.name", required: true),
                Address: row.Text("seller.address", required: true),
                TaxPayerId: row.TextOrNull("seller.tax_payer_id")),
            SellerBank = row.Text("seller_bank"),
            SenderName = row.Text("sender_name"),
            SenderTaxPayerId = row.Text("sender_tax_payer_id"),
            SenderCargoHandlingTerminal = row.Text("sender_cargo_handling_terminal"),
            // Buyer columns come in a different order than the seller's: name, tax payer id, address.
            Buyer = ReadBuyer(row),
            BuyerBank = row.Text("buyer_bank", required: true),
            ReceiverName = row.Text("receiver_name", required: true),
            ReceiverTaxPayerId = row.Text("receiver_tax_payer_id", required: true),
            ReceiverCargoHandlingTerminal = row.Text("receiver_cargo_handling_terminal", required: true),
            Currency = row.Text("currency", required: true),
            ItemName = row.Text("item_name", required: true),
            Item = new WaybillItem(
                FeacnCode: row.Text("item.feacn_code", maxLength: FeacnLength),
                Price: row.Decimal("item.price")),
            PriceInNationalCurrency = row.Decimal("price_in_national_currency"),
            CorrectionDate = row.Date("correction_date"),
            GlobalUniqueIdentifier = row.Key("global_unique_identifier"),
        };

        return row.Errors.Count == 0 ? new(waybill, []) : new(null, row.Errors);
    }

    private static Party ReadBuyer(RowReader row)
    {
        var name = row.Text("buyer.name", required: true);
        var taxPayerId = row.TextOrNull("buyer.tax_payer_id");
        var address = row.Text("buyer.address", required: true);
        return new Party(name, address, taxPayerId);
    }
}
