namespace InventoryManagement.RequestModel
{
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }

    }

}
