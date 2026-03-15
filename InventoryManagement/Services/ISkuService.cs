namespace InventoryManagement.Services
{
    public interface ISkuService
    {
        Task<string> GenerateSkuAsync(int productId, string variantName);
        string GenerateBarcode(string sku);

    }
}
