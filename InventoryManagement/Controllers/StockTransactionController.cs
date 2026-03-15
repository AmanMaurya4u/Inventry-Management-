using InventoryManagement.Data;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockTransactionsController : ControllerBase
    {
        private readonly ImsDbContext _context;

        public StockTransactionsController(ImsDbContext context)
        {
            _context = context;
        }

        // GET: api/StockTransactions
        [HttpGet]
        public async Task<IActionResult> GetTransactions(DateTime? from, DateTime? to)
        {
            var response = new ResponseData();

            try
            {
                var query = _context.StockTransactions.AsQueryable();

                if (from.HasValue)
                    query = query.Where(t => t.TransactionDate >= from.Value);

                if (to.HasValue)
                    query = query.Where(t => t.TransactionDate <= to.Value);

                var transactions = await query
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new StockTransactionDto
                    {
                        StockTransactionId = t.StockTransactionId,
                        ProductVariantId = t.ProductVariantId,
                        WarehouseId = t.WarehouseId,
                        TransactionType = t.TransactionType,
                        Quantity = t.Quantity,
                        TransactionDate = t.TransactionDate,
                        ReferenceType = t.ReferenceType,
                        ReferenceId = t.ReferenceId,
                        Remarks = t.Remarks
                    })
                    .ToListAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Result = transactions;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // GET: api/StockTransactions/variant/5
        [HttpGet("variant/{variantId}")]
        public async Task<IActionResult> GetByVariant(int variantId)
        {
            var response = new ResponseData();

            try
            {
                var transactions = await _context.StockTransactions
                    .Where(t => t.ProductVariantId == variantId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new StockTransactionDto
                    {
                        StockTransactionId = t.StockTransactionId,
                        ProductVariantId = t.ProductVariantId,
                        WarehouseId = t.WarehouseId,
                        TransactionType = t.TransactionType,
                        Quantity = t.Quantity,
                        TransactionDate = t.TransactionDate,
                        ReferenceType = t.ReferenceType,
                        ReferenceId = t.ReferenceId,
                        Remarks = t.Remarks
                    })
                    .ToListAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Result = transactions;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // GET: api/StockTransactions/warehouse/3
        [HttpGet("warehouse/{warehouseId}")]
        public async Task<IActionResult> GetByWarehouse(int warehouseId)
        {
            var response = new ResponseData();

            try
            {
                var transactions = await _context.StockTransactions
                    .Where(t => t.WarehouseId == warehouseId)
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new StockTransactionDto
                    {
                        StockTransactionId = t.StockTransactionId,
                        ProductVariantId = t.ProductVariantId,
                        WarehouseId = t.WarehouseId,
                        TransactionType = t.TransactionType,
                        Quantity = t.Quantity,
                        TransactionDate = t.TransactionDate,
                        ReferenceType = t.ReferenceType,
                        ReferenceId = t.ReferenceId,
                        Remarks = t.Remarks
                    })
                    .ToListAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Result = transactions;
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