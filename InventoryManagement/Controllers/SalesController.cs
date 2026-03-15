using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ImsDbContext _context;
        public SalesController(ImsDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Sales([FromBody] SalesRequest request)
        {
            var response = new ResponseData();
            try
            {
                var warehouseExists = await _context.Warehouses
                    .AnyAsync(w => w.WarehouseId == request.WarehouseId);

                if (!warehouseExists)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid WarehouseId";
                    return BadRequest(response);
                }

                if (request.Status == "Completed")
                {
                    foreach (var i in request.Items)
                    {
                        var stock = await _context.ProductVariantStock
                            .FirstOrDefaultAsync(s =>
                                s.ProductVariantId == i.ProductVariantId &&
                                s.WarehouseId == request.WarehouseId);

                        if (stock == null || stock.Quantity < i.Quantity)
                        {
                            response.StatusCode = 400;
                            response.Message = $"Insufficient stock for variant {i.ProductVariantId}";
                            return BadRequest(response);
                        }
                    }
                }

                var sale = new Sale
                {
                    WarehouseId = request.WarehouseId,
                    SaleDate = request.SaleDate,
                    Status = request.Status,
                    Items = request.Items.Select(i => new SalesItem
                    {
                        ProductVariantId = i.ProductVariantId,
                        Quantity = i.Quantity,
                        SellingPrice = i.SellingPrice,
                        Total = i.Quantity * i.SellingPrice
                    }).ToList()
                };

                sale.TotalAmount = sale.Items.Sum(i => i.Total);

                _context.Sales.Add(sale);

                if (sale.Status == "Completed")
                {
                    foreach (var item in sale.Items)
                    {
                        var stock = await _context.ProductVariantStock
                            .FirstAsync(s =>
                                s.ProductVariantId == item.ProductVariantId &&
                                s.WarehouseId == sale.WarehouseId);

                        stock.Quantity -= item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Sale saved and inventory updated";
                response.Result = sale.SaleId;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        public async Task<IActionResult> GetSales()
        {
            var response = new ResponseData();
            try
            {
                var sales = await _context.Sales
                .Include(p => p.Items)
                .Select(p => new SalesResponse
                {
                    SaleId = p.SaleId,
                    WarehouseId = p.WarehouseId,
                    SaleDate = p.SaleDate,
                    Status = p.Status,
                    TotalAmount = p.TotalAmount,
                    Items = p.Items.Select(i => new SalesItemDto
                    {
                        SalesItemId = i.SalesItemId,
                        ProductVariantId = i.ProductVariantId,
                        Quantity = i.Quantity,
                        SellingPrice = i.SellingPrice,
                        Total = i.Total
                    }).ToList() 

                }).ToListAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "All Sales Statement Retrieved Successfully";
                response.Result = sales;
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
