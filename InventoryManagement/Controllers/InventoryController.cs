using InventoryManagement.Data;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly ImsDbContext _context;
        public InventoryController(ImsDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetInventory()
        {
            var response = new ResponseData();
            try
            {
                var inventory = await _context.ProductVariantStock
                .Include(i => i.ProductVariant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.Category)
                .Include(i => i.Warehouse)
                .Select(i => new InventoryResponse
                {
                    ProductVariantId = i.ProductVariantId,
                    SKU = i.ProductVariant.SKU,
                    VariantName = i.ProductVariant.VariantName,
                    ProductName = i.ProductVariant.Product.Name,
                    CategoryName = i.ProductVariant.Product.Category.Name,
                    WarehouseId = i.WarehouseId,
                    WarehouseName = i.Warehouse.Name,
                    Quantity = i.Quantity
                })
                .ToListAsync();
                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Inventory Data Retrieved";
                response.Result = inventory;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
    }
}
