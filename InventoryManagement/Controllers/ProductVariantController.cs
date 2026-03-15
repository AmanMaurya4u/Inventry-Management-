using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using InventoryManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductVariantController : ControllerBase
    {
        private readonly ImsDbContext _context;
        private readonly ISkuService _skuService;
        public ProductVariantController(ImsDbContext context, ISkuService skuService)
        {
            _skuService = skuService;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetProductVariants()
        {
            var response = new ResponseData();
            try
            {
                var varient = await _context.ProductVariants.
                    Select(p => new ProductVariantResponse
                    {
                        ProductVariantId = p.ProductVariantId,
                        ProductId = p.ProductId,
                        VariantName = p.VariantName,
                        SKU = p.SKU,
                        Barcode = p.Barcode,
                        Unit = p.Unit,
                        CostPrice = p.CostPrice,
                        SellingPrice = p.SellingPrice,
                        MinStockLevel = p.MinStockLevel,
                        IsActive = p.IsActive,

                    }).ToListAsync();
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = "Product's Variant List Retrieved Successfully";
                response.Result = varient;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{VariantId}")]
        public async Task<IActionResult> GetProductVariant(int VariantId)
        {
            var response = new ResponseData();
            try
            {
                var variant = await _context.ProductVariants.FindAsync(VariantId);  
                if(variant == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Variant Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = "Variant Retrieved Successfully";
                var result = new ProductVariantResponse
                {
                    ProductVariantId = variant.ProductVariantId,
                    ProductId = variant.ProductId,
                    VariantName = variant.VariantName,
                    SKU = variant.SKU,
                    Barcode = variant.Barcode,
                    Unit = variant.Unit,
                    CostPrice = variant.CostPrice,
                    SellingPrice = variant.SellingPrice,
                    MinStockLevel = variant.MinStockLevel,
                    IsActive = variant.IsActive,
                };
                response.Result = result;
            }
            catch(Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        public async Task<IActionResult> AddVariant([FromBody] ProductVariantRequest request)
        {
            var response = new ResponseData();
            try
            {
                var productExists = await _context.Products
                    .AnyAsync(p => p.ProductId == request.ProductId);

                if (!productExists)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid ProductId";
                    return StatusCode(response.StatusCode, response);
                }

                var name = request.VariantName.Trim();

                bool exists = await _context.ProductVariants
                    .AnyAsync(x => x.ProductId == request.ProductId &&
                                   x.VariantName == name);

                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Variant Already Exists";
                    return StatusCode(response.StatusCode, response);
                }

                var sku = await _skuService.GenerateSkuAsync(request.ProductId, name);

                var variant = new ProductVariant
                {
                    ProductId = request.ProductId,
                    VariantName = name,
                    Unit = request.Unit,
                    CostPrice = request.CostPrice,
                    SellingPrice = request.SellingPrice,
                    MinStockLevel = request.MinStockLevel,
                    IsActive = true,
                    SKU = sku,
                    Barcode = _skuService.GenerateBarcode(sku)
                };

                await _context.ProductVariants.AddAsync(variant);
                await _context.SaveChangesAsync();

                response.StatusCode = 201;
                response.IsSuccess = true;
                response.Message = "Variant Added Successfully";
                response.Result = new ProductVariantResponse
                {
                    ProductVariantId = variant.ProductVariantId,
                    ProductId = variant.ProductId,
                    VariantName = variant.VariantName,
                    SKU = variant.SKU,
                    Barcode = variant.Barcode,
                    Unit = variant.Unit,
                    CostPrice = variant.CostPrice,
                    SellingPrice = variant.SellingPrice,
                    MinStockLevel = variant.MinStockLevel,
                    IsActive = variant.IsActive
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("{variantId}")]
        public async Task<IActionResult> UpdateVariant([FromBody] ProductVariantRequest request, int variantId)
        {
            var response = new ResponseData();
            try
            {
                var variant = await _context.ProductVariants.FindAsync(variantId);
                if(variant == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Variant Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                if (variant.ProductId != request.ProductId)
                {
                    response.StatusCode = 400;
                    response.Message = "Product cannot be changed for existing variant";
                    return StatusCode(response.StatusCode, response);
                }

                var name = request.VariantName.Trim();

                bool exists = await _context.ProductVariants
                    .AnyAsync(x => x.ProductId == variant.ProductId &&
                                   x.VariantName == name &&
                                   x.ProductVariantId != variantId);

                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Variant Already Exists";
                    return StatusCode(response.StatusCode, response);
                }
                

                variant.VariantName = name;
                variant.Unit = request.Unit;
                variant.CostPrice = request.CostPrice;
                variant.SellingPrice = request.SellingPrice;
                variant.MinStockLevel = request.MinStockLevel;
                variant.IsActive = request.IsActive;

                var sku = await _skuService.GenerateSkuAsync(request.ProductId, name);
                variant.SKU = sku;
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Variant Updated Successfully";
                response.Result = new ProductVariantResponse
                {
                    ProductVariantId = variant.ProductVariantId,
                    ProductId = variant.ProductId,
                    VariantName = variant.VariantName,
                    SKU = variant.SKU,
                    Barcode = variant.Barcode,
                    Unit = variant.Unit,
                    CostPrice = variant.CostPrice,
                    SellingPrice = variant.SellingPrice,
                    MinStockLevel = variant.MinStockLevel,
                    IsActive = variant.IsActive
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{variantId}")]
        public async Task<IActionResult> DeleteVariant(int variantId)
        {
            var response = new ResponseData();
            try
            {
                var variant = await _context.ProductVariants.FindAsync(variantId);

                if (variant == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Variant Not Found";
                    return StatusCode(response.StatusCode, response);
                }

                variant.IsActive = false;

                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Variant Deactivated Successfully";
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
