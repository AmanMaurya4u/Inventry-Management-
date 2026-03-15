namespace InventoryManagement.Data
{
    public class Warehouse
    {
        public int WarehouseId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public bool IsActive { get; set; }

        public ICollection<ProductVariantStock> VariantStocks { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
        public ICollection<Sale> Sales { get; set; }
    }

}
