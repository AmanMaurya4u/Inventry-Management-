using Azure.Core;
using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly ImsDbContext _context;
        public StockController(ImsDbContext context)
        {
            _context = context;
        }
        [HttpPost("transfer")]
        public async Task<IActionResult> StockTransfer([FromBody] StockTransferRequest stock)
        {
            var response = new ResponseData();
            try
            {
                if (stock.FromWarehouseId == stock.ToWarehouseId)
                {
                    response.StatusCode = 400;
                    response.Message = "Source and destination warehouse cannot be the same";
                    return StatusCode(response.StatusCode, response);
                }
                var warehousesExist = await _context.Warehouses
                    .Where(w => w.WarehouseId == stock.FromWarehouseId || w.WarehouseId == stock.ToWarehouseId)
                    .Select(w => w.WarehouseId)
                    .ToListAsync();

                if (!warehousesExist.Contains(stock.FromWarehouseId) ||
                    !warehousesExist.Contains(stock.ToWarehouseId))
                {
                    response.StatusCode = 404;
                    response.Message = "One or both warehouses not found";
                    return StatusCode(response.StatusCode, response);
                }
                if (!await _context.ProductVariants.AnyAsync(x => x.ProductVariantId == stock.ProductVariantId))
                {
                    response.StatusCode = 404;
                    response.Message = "Product Variant with the given Id Does'nt Exists";
                    return StatusCode(response.StatusCode, response);
                }

                var sourceStock = await _context.ProductVariantStock
                .FirstOrDefaultAsync(s =>
                    s.ProductVariantId == stock.ProductVariantId &&
                    s.WarehouseId == stock.FromWarehouseId);

                if (sourceStock == null || sourceStock.Quantity < stock.Quantity)
                {
                    response.StatusCode = 400;
                    response.Message = "Insufficient stock in source warehouse";
                    return StatusCode(response.StatusCode, response);
                }
                // SOURCE warehouse decrease
                using var tx = await _context.Database.BeginTransactionAsync();

                sourceStock.Quantity -= stock.Quantity;

                var destStock = await _context.ProductVariantStock
                    .FirstOrDefaultAsync(s =>
                        s.ProductVariantId == stock.ProductVariantId &&
                        s.WarehouseId == stock.ToWarehouseId);

                if (destStock != null)
                {
                    destStock.Quantity += stock.Quantity;
                }
                else
                {
                    _context.ProductVariantStock.Add(new ProductVariantStock
                    {
                        ProductVariantId = stock.ProductVariantId,
                        WarehouseId = stock.ToWarehouseId,
                        Quantity = stock.Quantity
                        
                    });
                }

                _context.StockTransactions.AddRange(
                new StockTransaction
                {
                    ProductVariantId = stock.ProductVariantId,
                    WarehouseId = stock.FromWarehouseId,
                    Quantity = -stock.Quantity,   // OUT = negative
                    TransactionType = TransactionTypes.TransferOut,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceType = TransactionTypes.Purchase,
                    Remarks = $"Transfer to warehouse {stock.ToWarehouseId}"
                },
                new StockTransaction
                {
                    ProductVariantId = stock.ProductVariantId,
                    WarehouseId = stock.ToWarehouseId,
                    Quantity = stock.Quantity,    // IN = positive
                    TransactionType = TransactionTypes.TransferIn,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceType = TransactionTypes.Purchase,
                    Remarks = $"Transfer from warehouse {stock.FromWarehouseId}"
                });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.StatusCode = 200;
                response.Message = "Stock Transfered Successfully";
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("adjustment")]
        public async Task<IActionResult> Adjustment([FromBody] AdjustmentRequest request)
        {
            var response = new ResponseData();
            try
            {
                if (!await _context.ProductVariants.AnyAsync(x =>
                x.ProductVariantId == request.ProductVariantId))
                {
                    response.StatusCode = 404;
                    response.Message = "Product Variant Does'nt Exists";
                    return StatusCode(response.StatusCode, response);
                }

                if (!await _context.Warehouses.AnyAsync(x =>
                x.WarehouseId == request.WarehouseId))
                {
                    response.StatusCode = 404;
                    response.Message = "Warehouse Does'nt Exists";
                    return StatusCode(response.StatusCode, response);
                }

                var stock = await _context.ProductVariantStock
               .FirstOrDefaultAsync(s =>
                   s.ProductVariantId == request.ProductVariantId &&
                   s.WarehouseId == request.WarehouseId);

                if (request.AdjustmentType == TransactionTypes.AdjustmentOut)
                {
                    if (stock == null || stock.Quantity < request.Quantity)
                    {
                        response.StatusCode = 400;
                        response.Message = "Insufficient stock in source warehouse";
                        return StatusCode(response.StatusCode, response);
                    }
                    stock.Quantity -= request.Quantity;
                }
                else
                {
                    if (stock != null)
                    {
                        stock.Quantity += request.Quantity;
                    }
                    else
                    {
                        _context.ProductVariantStock.Add(new ProductVariantStock
                        {
                            ProductVariantId = request.ProductVariantId,
                            WarehouseId = request.WarehouseId,
                            Quantity = request.Quantity
                        });
                    }
                }
                var signedQty = request.AdjustmentType == TransactionTypes.AdjustmentOut
                    ? -request.Quantity
                    : request.Quantity;

                var transaction = new StockTransaction
                {
                    ProductVariantId = request.ProductVariantId,
                    WarehouseId = request.WarehouseId,
                    Quantity = signedQty,
                    TransactionType = request.AdjustmentType,
                    TransactionDate = DateTime.UtcNow,
                    ReferenceType = request.ReferenceType,
                    ReferenceId = request.ReferenceId,
                    Remarks = request.Remarks
                };

                await _context.StockTransactions.AddAsync(transaction);
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Stock adjusted successfully";
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var response = new ResponseData();

            try
            {
                var data = await _context.StockTransactions
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

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Result = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // -------------------------------------------------------
        // GET: /api/stock/transactions/variant/{id}
        // -------------------------------------------------------
        [HttpGet("transactions/variant/{variantId}")]
        public async Task<IActionResult> GetVariantTransactions(int variantId)
        {
            var response = new ResponseData();

            try
            {
                var data = await _context.StockTransactions
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

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Result = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // -------------------------------------------------------
        // GET: /api/stock/transactions/warehouse/{id}
        // -------------------------------------------------------
        [HttpGet("transactions/warehouse/{warehouseId}")]
        public async Task<IActionResult> GetWarehouseTransactions(int warehouseId)
        {
            var response = new ResponseData();

            try
            {
                var data = await _context.StockTransactions
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

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Result = data;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // -------------------------------------------------------
        // GET: /api/stock/inventory/summary
        // -------------------------------------------------------
        [HttpGet("inventory/summary")]
        public async Task<IActionResult> GetInventorySummary()
        {
            var response = new ResponseData();

            try
            {
                var variants = await _context.ProductVariants
                    .Select(v => new
                    {
                        v.ProductVariantId,
                        v.MinStockLevel,
                        TotalQuantity = _context.ProductVariantStock
                            .Where(s => s.ProductVariantId == v.ProductVariantId)
                            .Sum(s => (decimal?)s.Quantity) ?? 0
                    })
                    .ToListAsync();

                var totalSkus = variants.Count;

                var outOfStockCount = variants.Count(v => v.TotalQuantity <= 0);

                var lowStockCount = variants.Count(v =>
                    v.TotalQuantity > 0 &&
                    v.TotalQuantity <= v.MinStockLevel
                );

                var healthySkuCount = variants.Count(v =>
                    v.TotalQuantity > v.MinStockLevel
                );

                var summary = new InventorySummaryDto
                {
                    TotalSkus = totalSkus,
                    HealthySkuCount = healthySkuCount,
                    LowStockCount = lowStockCount,
                    OutOfStockCount = outOfStockCount
                };

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Result = summary;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        // -------------------------------------------------------
        // GET: /api/stock/dashboard/stock-movements?days=7
        // -------------------------------------------------------
        [HttpGet("dashboard/stock-movements")]
        public async Task<IActionResult> GetStockMovements(int days = 7)
        {
            var response = new ResponseData();

            try
            {
                var fromDate = DateTime.UtcNow.AddDays(-days);

                var data = await _context.StockTransactions
                    .Where(t => t.TransactionDate >= fromDate)
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => new StockMovementDto
                    {
                        Date = g.Key,
                        TotalIn = g.Where(x => x.Quantity > 0).Sum(x => x.Quantity),
                        TotalOut = g.Where(x => x.Quantity < 0).Sum(x => Math.Abs(x.Quantity))
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Result = data;
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
