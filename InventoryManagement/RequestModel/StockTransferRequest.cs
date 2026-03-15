namespace InventoryManagement.RequestModel
{
    public class StockTransferRequest
    {
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TransferDate { get; set; }
        
    }
}
