namespace InventoryManagement.Data
{
    public class SalesItem
    {
        public int SalesItemId { get; set; }
        public int SaleId { get; set; }
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal Total { get; set; }

        public Sale Sale { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }
}
