namespace InventoryManagement.Data
{
    public class StockTransferItem
    {
        public int StockTransferItemId { get; set; }
        public int StockTransferId { get; set; }
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public StockTransfer StockTransfer { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }

}
