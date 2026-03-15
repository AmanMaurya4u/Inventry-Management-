namespace InventoryManagement.RequestModel
{
    public class AdjustmentRequest
    {
        public int ProductVariantId { get; set; }
        public int WarehouseId { get; set; }
        public string AdjustmentType { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? AdjustmentDate { get; set; }
        public string ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public string Remarks { get; set; }
    }
}
