public class PurchaseResponse
{
    public int PurchaseId { get; set; }
    public int SupplierId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }

    public List<PurchaseItemDto> Items { get; set; }
}

public class PurchaseItemDto
{
    public int PurchaseItemId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal Total { get; set; }
}