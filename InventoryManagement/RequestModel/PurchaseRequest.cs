namespace InventoryManagement.RequestModel
{
    public class PurchaseRequest
    {
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime PurchaseDate { get; set; }

        // Draft / Received
        public string Status { get; set; }

        public List<PurchaseItemDto> Items { get; set; }
    }

    public class PurchaseItemDto
    {
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
