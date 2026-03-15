using InventoryManagement.Data;

namespace InventoryManagement.RequestModel
{
    public class CategoryRequest
    {
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

}
