namespace InventoryManagement.Data
{
    public class StockTransfer
    {
        public int StockTransferId { get; set; }
        public int FromWarehouseId { get; set; }
        public int ToWarehouseId { get; set; }
        public DateTime TransferDate { get; set; }
        public string Status { get; set; }

        public Warehouse FromWarehouse { get; set; }
        public Warehouse ToWarehouse { get; set; }
        public ICollection<StockTransferItem> Items { get; set; }
    }

}
