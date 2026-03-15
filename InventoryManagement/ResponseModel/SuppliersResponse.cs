using InventoryManagement.Data;

namespace InventoryManagement.RequestModel
{
    public class SupplierResponse
    {
        public int SupplierId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxId { get; set; }
        public bool IsActive { get; set; }
      
    }

}
