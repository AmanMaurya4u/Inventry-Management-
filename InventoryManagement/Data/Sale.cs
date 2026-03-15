namespace InventoryManagement.Data
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime SaleDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        public Warehouse Warehouse { get; set; }
        public ICollection<SalesItem> Items { get; set; }
    }

}
