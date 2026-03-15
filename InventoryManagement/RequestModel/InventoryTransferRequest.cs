namespace InventoryManagement.RequestModel
{
    public class InventoryTransferRequest
    {
        public int ProductVariantId { get; set; }

        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }

        public decimal Quantity { get; set; }
    }
}
