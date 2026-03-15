namespace InventoryManagement.Data
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int? DefaultSupplierId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

        public Category Category { get; set; }
        public Supplier DefaultSupplier { get; set; } 
        public ICollection<ProductVariant> Variants { get; set; }
    }

}
