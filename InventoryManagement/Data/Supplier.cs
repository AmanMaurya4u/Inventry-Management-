namespace InventoryManagement.Data
{
    public class Supplier
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxId { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Product> Products { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
    }

}
