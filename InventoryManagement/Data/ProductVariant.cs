namespace InventoryManagement.Data
{
    public class ProductVariant
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
        public Product Product { get; set; }
        public ICollection<ProductVariantStock> Stocks { get; set; }
        public ICollection<PurchaseItem> PurchaseItems { get; set; }
        public ICollection<SalesItem> SalesItems { get; set; }
        public ICollection<StockTransferItem> TransferItems { get; set; }
        public ICollection<StockTransaction> StockTransactions { get; set; }
    }

}
