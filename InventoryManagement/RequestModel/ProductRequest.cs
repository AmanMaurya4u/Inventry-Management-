using InventoryManagement.Data;

namespace InventoryManagement.RequestModel
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int? DefaultSupplierId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }


}
