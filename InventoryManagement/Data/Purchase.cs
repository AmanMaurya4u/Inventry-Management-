namespace InventoryManagement.Data
{
    public class Purchase
    {
        public int PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        public Supplier Supplier { get; set; }
        public Warehouse Warehouse { get; set; }
        public ICollection<PurchaseItem> Items { get; set; }
    }

}
