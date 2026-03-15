namespace InventoryManagement.Data
{
    public class PurchaseItem
    {
        public int PurchaseItemId { get; set; }
        public int PurchaseId { get; set; }
        public int ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal Total { get; set; }

        public Purchase Purchase { get; set; }
        public ProductVariant ProductVariant { get; set; }
    }

}
