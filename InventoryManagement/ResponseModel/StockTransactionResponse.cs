namespace InventoryManagement.ResponseModel
{
    public class StockTransactionDto
    {
        public int StockTransactionId { get; set; }
        public int ProductVariantId { get; set; }
        public int WarehouseId { get; set; }
        public string TransactionType { get; set; }
        public decimal Quantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string Remarks { get; set; }
    }
}