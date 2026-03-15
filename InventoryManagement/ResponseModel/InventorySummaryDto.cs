namespace InventoryManagement.ResponseModel
{
    public class InventorySummaryDto
    {
        public int TotalSkus { get; set; }
        public int HealthySkuCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }
}
