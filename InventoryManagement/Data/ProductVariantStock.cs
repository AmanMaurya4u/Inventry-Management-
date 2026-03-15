namespace InventoryManagement.Data
{
    public class ProductVariantStock
    {
        public int ProductVariantStockId { get; set; }
        public int ProductVariantId { get; set; }
        public int WarehouseId { get; set; }
        public decimal Quantity { get; set; }
        public ProductVariant ProductVariant { get; set; }
        public Warehouse Warehouse { get; set; }
    }

}
