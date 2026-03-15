using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ImsDbContext _context;

        public CategoriesController(ImsDbContext context)
        {
            this._context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var response = new ResponseData();

            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryResponse
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,  
                        ParentCategoryId = c.ParentCategoryId,
                        ParentCategoryName = c.ParentCategory != null
                            ? c.ParentCategory.Name
                            : null,
                        Description = c.Description,
                        IsActive = c.IsActive,
                    })
                    .ToListAsync();

                response.StatusCode = 200;
                response.Message = "Here's the list";
                response.IsSuccess = true;
                response.Result = categories;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategory(int categoryId)
        {
            var response = new ResponseData();

            try
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    response.StatusCode = 404;
                    response.Message = $"Could not find category {categoryId}";
                }
                else
                {
                    var result = new CategoryResponse
                    {
                        CategoryId = category.CategoryId,
                        Name = category.Name,
                        ParentCategoryId = category.ParentCategoryId,
                        Description = category.Description,
                        IsActive = category.IsActive
                        
                    };
                    response.StatusCode = 200;
                    response.Message = "Here's the list";
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
        public async Task<IActionResult> AddCategory([FromBody] CategoryRequest request)
        {
            var response = new ResponseData();

            try
            {
                // validate parent exists
                if (request.ParentCategoryId != null)
                {
                    var parentExists = await _context.Categories
                        .AnyAsync(c => c.CategoryId == request.ParentCategoryId);

                    if (!parentExists)
                    {
                        response.StatusCode = 400;
                        response.Message = "Parent category does not exist";
                        response.IsSuccess = false;
                        return StatusCode(response.StatusCode, response);
                    }
                }

                // prevent duplicate under same parent
                var exists = await _context.Categories.AnyAsync(c =>
                    c.Name == request.Name &&
                    c.ParentCategoryId == request.ParentCategoryId);

                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Category already exists in this parent";
                    response.IsSuccess = false;
                    return StatusCode(response.StatusCode, response);
                }

                var category = new Category
                {
                    Name = request.Name,
                    ParentCategoryId = request.ParentCategoryId,
                    Description = request.Description,
                    IsActive = true
                };

                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();

                var result = new CategoryResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    ParentCategoryId = category.ParentCategoryId,
                    Description = category.Description
                };

                response.StatusCode = 201;
                response.Message = "Category created successfully";
                response.IsSuccess = true;
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
                response.IsSuccess = false;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryRequest request, int categoryId)
        {
            var response = new ResponseData();

            try
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new ResponseData
                    {
                        StatusCode = 404,
                        Message = "Category Not Found"
                    });

                }
                if (request.ParentCategoryId == categoryId)
                {
                    response.StatusCode = 400;
                    response.Message = "Category cannot be its own parent";
                    response.IsSuccess = false;
                    return StatusCode(response.StatusCode, response);
                }
                // validate parent exists
                if (request.ParentCategoryId != null)
                {
                    var parentExists = await _context.Categories
                        .AnyAsync(c => c.CategoryId == request.ParentCategoryId);

                    if (!parentExists)
                    {
                        response.StatusCode = 400;
                        response.Message = "Parent category does not exist";
                        response.IsSuccess = false;
                        return StatusCode(response.StatusCode, response);
                    }
                }
                // prevent duplicate under same parent
                var exists = await _context.Categories.AnyAsync(c =>
                    c.Name == request.Name &&
                    c.ParentCategoryId == request.ParentCategoryId && c.CategoryId != categoryId);
                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Category already exists in this parent";
                    response.IsSuccess = false;
                    return StatusCode(response.StatusCode, response);
                }
                else
                {
                    category.Name = request.Name;
                    category.ParentCategoryId = request.ParentCategoryId;
                    category.Description = request.Description;
                    category.IsActive = request.IsActive;
                }
                await _context.SaveChangesAsync();

                var result = new CategoryResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    ParentCategoryId = category.ParentCategoryId,
                    Description = category.Description
                };

                response.StatusCode = 200;
                response.Message = "Category updated successfully";
                response.IsSuccess = true;
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var response = new ResponseData();
            try
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Category Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                // check children
                var hasChildren = await _context.Categories
                    .AnyAsync(c => c.ParentCategoryId == categoryId);

                if (hasChildren)
                {
                    response.StatusCode = 400;
                    response.Message = "Cannot delete category with subcategories";
                    return StatusCode(response.StatusCode, response);
                }

                // check products
                var hasProducts = await _context.Products
                    .AnyAsync(p => p.CategoryId == categoryId);

                if (hasProducts)
                {
                    response.StatusCode = 400;
                    response.Message = "Cannot delete category with products";
                    return StatusCode(response.StatusCode, response);
                }
                response.Result = category;

                _context.Remove(category);
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Category Deleted";
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
