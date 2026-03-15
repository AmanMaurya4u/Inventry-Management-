using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ImsDbContext _context;
        public ProductsController (ImsDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var response = new ResponseData();
            try
            {
                var products = await _context.Products.
                    Include(c => c.Category).
                    Include(c => c.DefaultSupplier).
                    Select(c => new ProductResponse
                    {
                        ProductId = c.ProductId,
                        Name = c.Name,
                        CategoryId = c.CategoryId,  
                        CategoryName = c.Category.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        DefaultSupplierId = c.DefaultSupplierId,
                        SupplierName = c.DefaultSupplier != null
                            ? c.DefaultSupplier.Name
                            : null,

                    })
                    .ToListAsync();
                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = "Here's the list";
                response.Result = products;
            }

            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProduct(int productId)
        {
            var response = new ResponseData();
            try
            {
                var product = await _context.Products
                   .Include(p => p.Category)
                   .Include(p => p.DefaultSupplier)
                   .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Product Not Found";
                }
                else
                {
                    var result = new ProductResponse
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        CategoryName = product.Category.Name,
                        Description = product.Description,
                        IsActive = product.IsActive,
                        DefaultSupplierId = product.DefaultSupplierId,
                        SupplierName = product.DefaultSupplier?.Name,
                    };
                    response.StatusCode = 200;
                    response.Message = "Here's the Product";
                    response.IsSuccess = true;
                    response.Result = result;
                }
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductRequest product)
        {
            var response = new ResponseData();
            try
            {
                var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryId == product.CategoryId);

                if (!categoryExists)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid CategoryId";
                    return StatusCode(response.StatusCode, response);
                }

                if (product.DefaultSupplierId != null)
                {
                    var supplierExists = await _context.Suppliers
                        .AnyAsync(s => s.SupplierId == product.DefaultSupplierId);

                    if (!supplierExists)
                    {
                        response.StatusCode = 400;
                        response.Message = "Invalid SupplierId";
                        return StatusCode(response.StatusCode, response);
                    }
                }
                var exists = await _context.Products
                   .AnyAsync(x => x.Name == product.Name && 
                   x.CategoryId == product.CategoryId && x.DefaultSupplierId == product.DefaultSupplierId);

                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Product Already Exists";
                }
                else
                {
                    var prod = new Product
                    {
                        Name = product.Name,
                        CategoryId = product.CategoryId,
                        Description = product.Description,
                        DefaultSupplierId = product.DefaultSupplierId,
                        IsActive = true
                    };
                    
                    await _context.Products.AddAsync(prod);
                    await _context.SaveChangesAsync();

                    response.StatusCode = 201;
                    response.Message = "Product Added Successfully";
                    response.IsSuccess = true;
                    var result = new ProductResponse
                    {
                        ProductId = prod.ProductId,
                        Name = prod.Name,
                        CategoryId = prod.CategoryId,
                        DefaultSupplierId = prod.DefaultSupplierId,
                        Description = prod.Description,
                        IsActive = prod.IsActive
                    };
                    response.Result = result;   
                }

            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);

        }
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductRequest product, int productId)
        {
            var response = new ResponseData();
            try
            {
                var prod = await _context.Products.FindAsync(productId);
                if(prod == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Product Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                var categoryExists = await _context.Categories
                   .AnyAsync(c => c.CategoryId == product.CategoryId);

                if (!categoryExists)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid CategoryId";
                    return StatusCode(response.StatusCode, response);
                }

                if (product.DefaultSupplierId != null)
                {
                    var supplierExists = await _context.Suppliers
                        .AnyAsync(s => s.SupplierId == product.DefaultSupplierId);

                    if (!supplierExists)
                    {
                        response.StatusCode = 400;
                        response.Message = "Invalid SupplierId";
                        return StatusCode(response.StatusCode, response);
                    }
                }
                var alreadyExists = await _context.Products
                   .AnyAsync(x => x.Name == product.Name &&
                   x.CategoryId == product.CategoryId && x.DefaultSupplierId == product.DefaultSupplierId && x.ProductId != productId);

                if (alreadyExists)
                {
                    response.StatusCode = 400;
                    response.Message = "Product Already Exists";
                }
                else
                {
                    prod.Name = product.Name;
                    prod.CategoryId = product.CategoryId;
                    prod.Description = product.Description;
                    prod.DefaultSupplierId = product.DefaultSupplierId;
                    prod.IsActive = product.IsActive;

                    await _context.SaveChangesAsync();

                    response.StatusCode = 200;
                    response.Message = "Product Updated Successfully";
                    response.IsSuccess = true;
                    var result = new ProductResponse
                    {
                        ProductId = prod.ProductId,
                        Name = prod.Name,
                        CategoryId = prod.CategoryId,
                        DefaultSupplierId = prod.DefaultSupplierId,
                        Description = prod.Description,
                        IsActive = prod.IsActive
                    };
                    response.Result = result;
                }

            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);

        }
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var response = new ResponseData();
            try
            {
                var prod = await _context.Products.FindAsync(productId);
                if (prod == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Product Not Found";
                    return StatusCode(response.StatusCode, response);
                }

                prod.IsActive = false;
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Product Deleted Successfully";
                response.IsSuccess = true;
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
