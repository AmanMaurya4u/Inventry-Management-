public class SalesResponse
{
    public int SaleId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime SaleDate { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    
    public List<SalesItemDto> Items { get; set; }
}

public class SalesItemDto
{
    public int SalesItemId { get; set; }
    public int ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal Total { get; set; }
}