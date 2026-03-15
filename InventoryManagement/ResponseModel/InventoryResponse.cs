namespace InventoryManagement.ResponseModel
{
    public class InventoryResponse
    {
        public int ProductVariantStockId { get; set; }
        public int ProductVariantId { get; set; }

        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }

        public string VariantName { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseName { get; set; }

        public decimal Quantity { get; set; }
    }
}
