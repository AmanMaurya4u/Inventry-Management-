namespace InventoryManagement.ResponseModel
{
    public class StockMovementDto
    {
        public DateTime Date { get; set; }
        public decimal TotalIn { get; set; }
        public decimal TotalOut { get; set; }
    }
}
