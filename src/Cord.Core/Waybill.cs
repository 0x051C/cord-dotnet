namespace Cord.Core;

/// <summary>A party named on a waybill: the seller or the buyer.</summary>
public sealed record Party(string Name, string Address, string? TaxPayerId);

/// <summary>The goods line of a waybill.</summary>
/// <param name="FeacnCode">Customs commodity code (FEACN / TN VED), at most 10 digits.</param>
public sealed record WaybillItem(string FeacnCode, decimal? Price);

/// <summary>
/// A waybill (TTN), one row of a <c>ttn_*.csv</c> file.
/// Properties are declared in source column order; <see cref="Flat"/> relies on it.
/// </summary>
public sealed record Waybill
{
    public required string Number { get; init; }
    public required DateOnly Registration { get; init; }
    /// <summary>Stated purpose of the shipment (sale, export, import...); used to route records to departments.</summary>
    public required string StatedGoal { get; init; }
    public DateOnly? CrossedAt { get; init; }
    public string Contract { get; init; } = "";
    public string Specification { get; init; } = "";
    public required Party Seller { get; init; }
    public string SellerBank { get; init; } = "";
    public string SenderName { get; init; } = "";
    public string SenderTaxPayerId { get; init; } = "";
    public string SenderCargoHandlingTerminal { get; init; } = "";
    public required Party Buyer { get; init; }
    public required string BuyerBank { get; init; }
    public required string ReceiverName { get; init; }
    public required string ReceiverTaxPayerId { get; init; }
    public required string ReceiverCargoHandlingTerminal { get; init; }
    public required string Currency { get; init; }
    public required string ItemName { get; init; }
    public required WaybillItem Item { get; init; }
    public decimal? PriceInNationalCurrency { get; init; }
    public DateOnly? CorrectionDate { get; init; }
    /// <summary>Unique per waybill in the source system; the Dataverse alternate key.</summary>
    public required string GlobalUniqueIdentifier { get; init; }
}
