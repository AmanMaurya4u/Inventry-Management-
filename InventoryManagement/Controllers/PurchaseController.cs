using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly ImsDbContext _context;

        public PurchaseController(ImsDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddPurchase([FromBody] PurchaseRequest request)
        {
            var response = new ResponseData();

            try
            {
                if (request.Items.Any(i => i.Quantity <= 0 || i.CostPrice <= 0))
                {
                    response.StatusCode = 400;
                    response.Message = "Quantity and CostPrice must be greater than zero";
                    return StatusCode(response.StatusCode, response);
                }
                // ✅ Validate Supplier
                if (!await _context.Suppliers.AnyAsync(x => x.SupplierId == request.SupplierId))
                {
                    response.StatusCode = 404;
                    response.Message = "Supplier not found";
                    return StatusCode(response.StatusCode, response);
                }

                // ✅ Validate Warehouse
                if (!await _context.Warehouses.AnyAsync(x => x.WarehouseId == request.WarehouseId))
                {
                    response.StatusCode = 404;
                    response.Message = "Warehouse not found";
                    return StatusCode(response.StatusCode, response);
                }

                // ✅ Validate ProductVariants
                var variantIds = request.Items.Select(i => i.ProductVariantId).ToList();
                var existingVariants = await _context.ProductVariants
                    .Where(v => variantIds.Contains(v.ProductVariantId))
                    .Select(v => v.ProductVariantId)
                    .ToListAsync();

                if (existingVariants.Count != variantIds.Count)
                {
                    response.StatusCode = 404;
                    response.Message = "One or more product variants not found";
                    return StatusCode(response.StatusCode, response);
                }

                // ✅ Create Purchase + Items
                var purchase = new Purchase
                {
                    SupplierId = request.SupplierId,
                    WarehouseId = request.WarehouseId,
                    PurchaseDate = request.PurchaseDate,
                    Status = request.Status,
                    Items = request.Items.Select(i => new PurchaseItem
                    {
                        ProductVariantId = i.ProductVariantId,
                        Quantity = i.Quantity,
                        CostPrice = i.CostPrice,
                        Total = i.Quantity * i.CostPrice
                    }).ToList()
                };

                purchase.TotalAmount = purchase.Items.Sum(i => i.Total);

                // ✅ Atomic transaction
                using var tx = await _context.Database.BeginTransactionAsync();

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync(); // generate PurchaseId

                // ✅ Update Inventory + StockTransaction only if Received
                if (purchase.Status.Equals("Received", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var item in purchase.Items)
                    {
                        var stock = await _context.ProductVariantStock
                            .FirstOrDefaultAsync(s =>
                                s.ProductVariantId == item.ProductVariantId &&
                                s.WarehouseId == purchase.WarehouseId);

                        if (stock != null)
                        {
                            stock.Quantity += item.Quantity;
                        }
                        else
                        {
                            _context.ProductVariantStock.Add(new ProductVariantStock
                            {
                                ProductVariantId = item.ProductVariantId,
                                WarehouseId = purchase.WarehouseId,
                                Quantity = item.Quantity
                            });
                        }

                        _context.StockTransactions.Add(new StockTransaction
                        {
                            ProductVariantId = item.ProductVariantId,
                            WarehouseId = purchase.WarehouseId,
                            Quantity = item.Quantity,
                            TransactionType = TransactionTypes.TransferIn,
                            TransactionDate = DateTime.UtcNow,
                            ReferenceType = TransactionTypes.Purchase,
                            ReferenceId = purchase.PurchaseId,
                            Remarks = "Stock added via purchase"
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                response.StatusCode = 201;
                response.IsSuccess = true;
                response.Message = "Purchase created successfully";
                response.Result = purchase.PurchaseId;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpGet]
        public async Task<IActionResult> GetPurchases()
        {
            var response = new ResponseData();
            try
            {
                var purchase = await _context.Purchases.
                    Select(p => new PurchaseResponse
                    {
                        PurchaseId = p.PurchaseId,
                        SupplierId = p.SupplierId,
                        WarehouseId = p.WarehouseId,
                        PurchaseDate = p.PurchaseDate,
                        Status = p.Status,
                        TotalAmount = p.TotalAmount,
                        Items = p.Items.Select(i => new PurchaseItemDto
                        {
                            PurchaseItemId = i.PurchaseItemId,
                            ProductVariantId = i.ProductVariantId,
                            Quantity = i.Quantity,
                            CostPrice = i.CostPrice,    
                            Total = i.Total
                        }).ToList(),
                    }).ToListAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "All Purchase Statement Retrieved Successfully";
                response.Result = purchase;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{purchaseId}")]
        public async Task<IActionResult> GetPurchaseById(int purchaseId)
        {
            var response = new ResponseData();
            try
            {
                 var purchase = await _context.Purchases
                .Where(p => p.PurchaseId == purchaseId)
                .Select(p => new PurchaseResponse
                {
                    PurchaseId = p.PurchaseId,
                    SupplierId = p.SupplierId,
                    WarehouseId = p.WarehouseId,
                    PurchaseDate = p.PurchaseDate,
                    Status = p.Status,
                    TotalAmount = p.TotalAmount,
                    Items = p.Items.Select(i => new PurchaseItemDto
                    {
                        PurchaseItemId = i.PurchaseItemId,
                        ProductVariantId = i.ProductVariantId,
                        Quantity = i.Quantity,
                        CostPrice = i.CostPrice,
                        Total = i.Total
                    }).ToList()
                })
                .FirstOrDefaultAsync();

                if (purchase == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Purchase not found";
                    return StatusCode(response.StatusCode, response);
                }

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Purchase Statement Retrieved Successfully";
                response.Result = purchase;
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