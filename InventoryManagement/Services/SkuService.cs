using Microsoft.EntityFrameworkCore;
using InventoryManagement.Data;

namespace InventoryManagement.Services
{
    public class SkuService : ISkuService
    {
        private readonly ImsDbContext _context;

        public SkuService(ImsDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateSkuAsync(int productId, string variantName)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new Exception("Product not found");

            string catCode = ToCode(product.Category?.Name);
            string prodCode = ToCode(product.Name);

            var (colorCode, sizeCode) = ExtractVariantParts(variantName);

            string prefix = $"{catCode}-{prodCode}-{colorCode}-{sizeCode}";

            var lastSku = await _context.ProductVariants
             .Where(v => v.ProductId == productId && v.SKU.StartsWith(prefix))
             .Select(v => v.SKU)
             .ToListAsync();

            int nextSeq = 1;

            if (lastSku.Any())
            {
                nextSeq = lastSku
                    .Select(s => int.Parse(s.Split('-').Last()))
                    .Max() + 1;
            }

            return $"{prefix}-{nextSeq:D5}";
        }

        private static string ToCode(string text, int length = 3)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "UNK";

            var clean = new string(text
                .Where(char.IsLetterOrDigit)
                .ToArray())
                .ToUpper();

            return clean.Length <= length
                ? clean
                : clean.Substring(0, length);
        }

        private static (string color, string size) ExtractVariantParts(string variantName)
        {
            if (string.IsNullOrWhiteSpace(variantName))
                return ("GEN", "STD");

            var parts = variantName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            string color = parts.ElementAtOrDefault(0) ?? "GEN";
            string size = parts.ElementAtOrDefault(1) ?? "STD";

            return (ToCode(color), ToCode(size));
        }
        public string GenerateBarcode(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return null;

            // Convert SKU to numeric hash
            var hash = Math.Abs(sku.GetHashCode());

            // Ensure fixed length (13 digits)
            return hash.ToString().PadLeft(13, '0').Substring(0, 13);
        }
    }
}
