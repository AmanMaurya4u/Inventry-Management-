namespace InventoryManagement.RequestModel
{
    
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public int? DefaultSupplierId { get; set; }
        public string? SupplierName { get; set; }

        public string Description { get; set; }
        public bool IsActive { get; set; }
    }


}
