namespace InventoryManagement.RequestModel
{
    public class InventoryRequest
    {
        public int ProductVariantId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
    
    }
}
