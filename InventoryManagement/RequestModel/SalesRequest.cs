namespace InventoryManagement.RequestModel
{
    public class SalesRequest
    {
        public int WarehouseId { get; set; }
        public DateTime SaleDate { get; set; }
        public string Status { get; set; }
        public List<SalesItemDto> Items { get; set; }

    }
    public class SalesItemDto
    {
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal SellingPrice { get; set; }
    }
}
