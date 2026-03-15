using InventoryManagement.Data;

namespace InventoryManagement.RequestModel
{
    public class ProductVariantResponse
    {
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public string VariantName { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string Unit { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal MinStockLevel { get; set; }
        public bool IsActive { get; set; }
    }
}
