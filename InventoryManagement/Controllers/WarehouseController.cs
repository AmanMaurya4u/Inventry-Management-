using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly ImsDbContext _context;
        public WarehouseController(ImsDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetWarehouses()
        {
            var response = new ResponseData();
            try
            {
                var warehouses = await _context.Warehouses.
                    Select(w => new WarehouseResponse
                    {
                        WarehouseId = w.WarehouseId,
                        Name = w.Name,
                        Location = w.Location,
                        IsActive = w.IsActive
                    }).ToListAsync();
                if (!warehouses.Any())
                {
                    response.StatusCode = 200;
                    response.IsSuccess = true;
                    response.Message = warehouses.Any()
                        ? "Warehouses retrieved successfully"
                        : "No warehouses found";
                    response.Result = warehouses;
                }
                else
                {
                    response.StatusCode = 200;
                    response.Message = "Warehouses retrieved successfully";
                    response.IsSuccess = true;
                    response.Result = warehouses;
                }

            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{warehouseId}")]
        public async Task<IActionResult> GetWarehouse(int warehouseId)
        {
            var response = new ResponseData();
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                if (warehouse == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Warehouse Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                var result = new WarehouseResponse
                {
                    WarehouseId = warehouse.WarehouseId,
                    Name = warehouse.Name,
                    Location = warehouse.Location,
                    IsActive = warehouse.IsActive

                };
                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Warehouse retrieved successfully";
                response.Result = result;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost]
        public async Task<IActionResult> AddWarehouse([FromBody] WarehouseRequest ware)
        {
            var response = new ResponseData();
            try
            {
                bool exists = await _context.Warehouses.AnyAsync(w => w.Name == ware.Name
                && w.Location == ware.Location);
                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Warehouse Already Exists";
                    return StatusCode(response.StatusCode, response);
                }
                var warehouse = new Warehouse
                {
                    Name = ware.Name,
                    Location = ware.Location,
                    IsActive = true
                };

                await _context.Warehouses.AddAsync(warehouse);
                await _context.SaveChangesAsync();

                response.StatusCode = 201;
                response.IsSuccess = true;
                response.Message = "Warehouse Created Successfully";
                response.Result = new WarehouseResponse
                {
                    WarehouseId = warehouse.WarehouseId,
                    Name = warehouse.Name,
                    Location = warehouse.Location,
                    IsActive = warehouse.IsActive,
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("{warehouseId}")]
        public async Task<IActionResult> UpdateWarehouse([FromBody] WarehouseRequest request, int warehouseId)
        {
            var response = new ResponseData();
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                if (warehouse == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Warehouse Does'nt Exists";
                    return StatusCode(response.StatusCode, response);
                }
                bool exists = await _context.Warehouses.AnyAsync(w => w.Name == request.Name
                && w.Location == request.Location && w.WarehouseId != warehouseId);
                if (exists)
                {
                    response.StatusCode = 400;
                    response.Message = "Warehouse Already Exists";
                    return StatusCode(response.StatusCode, response);
                }

                warehouse.Name = request.Name;
                warehouse.Location = request.Location;
                warehouse.IsActive = request.IsActive;

                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Warehouse Updated Successfully";
                response.Result = new WarehouseResponse
                {
                    WarehouseId = warehouse.WarehouseId,
                    Name = warehouse.Name,
                    Location = warehouse.Location,
                    IsActive = warehouse.IsActive,
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("{warehouseId}")]
        public async Task<IActionResult> DeleteWarehouse(int warehouseId)
        {
            var response = new ResponseData();
            try
            {
                var warehouse = await _context.Warehouses.FindAsync(warehouseId);
                if (warehouse == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Warehouse Not Found";
                    return StatusCode(response.StatusCode, response);
                }
                var hasStock = await _context.ProductVariantStock  
                     .AnyAsync(s => s.WarehouseId == warehouseId);

                if (hasStock)
                {
                    response.StatusCode = 400;
                    response.Message = "Warehouse has stock and cannot be deleted";
                    return StatusCode(response.StatusCode, response);
                }

                _context.Warehouses.Remove(warehouse);
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Warehouse Deleted Successfully";
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
