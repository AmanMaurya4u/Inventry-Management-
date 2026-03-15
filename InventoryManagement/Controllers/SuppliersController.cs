using InventoryManagement.Data;
using InventoryManagement.RequestModel;
using InventoryManagement.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ImsDbContext _context;

        public SuppliersController(ImsDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var response = new ResponseData();
            try
            {
                var suppliers = await _context.Suppliers
                    .Select(c => new SupplierResponse
                    {
                        SupplierId = c.SupplierId,
                        Name = c.Name,
                        Phone = c.Phone,
                        Email = c.Email,
                        Address = c.Address,
                        TaxId = c.TaxId,
                        IsActive = c.IsActive
                    })
                    .ToListAsync();

                response.IsSuccess = true;
                response.StatusCode = 200;
                response.Message = "Here's the Suppliers List";
                response.Result = suppliers;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        // named route so CreatedAtAction can reference it
        [HttpGet("{supplierId}", Name = "GetSupplier")]
        public async Task<IActionResult> GetSupplier(int supplierId)
        {
            var response = new ResponseData();
            try
            {
                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Supplier Not Found";
                    return StatusCode(response.StatusCode, response);
                }

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Here's the Supplier";
                response.Result = new SupplierResponse
                {
                    SupplierId = supplier.SupplierId,
                    Name = supplier.Name,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxId = supplier.TaxId,
                    IsActive = supplier.IsActive
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplier([FromBody] SupplierRequest request)
        {
            var response = new ResponseData();
            try
            {
                if (request == null)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid request";
                    return StatusCode(response.StatusCode, response);
                }

                bool duplicate = await _context.Suppliers.AnyAsync(x => x.TaxId == request.TaxId);
                if (duplicate)
                {
                    response.StatusCode = 400;
                    response.Message = "Supplier Already Exist";
                    return StatusCode(response.StatusCode, response);
                }

                var supplier = new Supplier
                {
                    Name = request.Name,
                    Phone = request.Phone,
                    Email = request.Email,
                    Address = request.Address,
                    TaxId = request.TaxId,
                    IsActive = true
                };

                await _context.Suppliers.AddAsync(supplier);
                await _context.SaveChangesAsync();

                // build response DTO with assigned SupplierId
                var result = new SupplierResponse
                {
                    SupplierId = supplier.SupplierId,
                    Name = supplier.Name,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxId = supplier.TaxId,
                    IsActive = supplier.IsActive
                };

                response.StatusCode = 201;
                response.Message = "Supplier Added Successfully";
                response.IsSuccess = true;
                response.Result = result;

                // return Created (201) with Location header
                return CreatedAtAction(nameof(GetSupplier), new { supplierId = supplier.SupplierId }, response);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }

            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{supplierId}")]
        public async Task<IActionResult> UpdateSupplier([FromBody] SupplierRequest sup, int supplierId)
        {
            var response = new ResponseData();
            try
            {
                if (sup == null)
                {
                    response.StatusCode = 400;
                    response.Message = "Invalid request";
                    return StatusCode(response.StatusCode, response);
                }

                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Supplier Not Found";
                    return StatusCode(response.StatusCode, response);
                }

                var duplicate = await _context.Suppliers
                    .AnyAsync(x => x.TaxId == sup.TaxId && x.SupplierId != supplierId);

                if (duplicate)
                {
                    response.StatusCode = 400;
                    response.Message = "Supplier with this TaxId already exists";
                    return StatusCode(response.StatusCode, response);
                }

                supplier.Name = sup.Name;
                supplier.Phone = sup.Phone;
                supplier.Email = sup.Email;
                supplier.Address = sup.Address;
                supplier.TaxId = sup.TaxId;
                supplier.IsActive = sup.IsActive;

                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.Message = "Updated Successfully";
                response.IsSuccess = true;
                response.Result = new SupplierResponse
                {
                    SupplierId = supplier.SupplierId,
                    Name = supplier.Name,
                    Phone = supplier.Phone,
                    Email = supplier.Email,
                    Address = supplier.Address,
                    TaxId = supplier.TaxId,
                    IsActive = supplier.IsActive
                };
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.Message = ex.Message;
            }
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Soft-delete supplier (set IsActive = false). Replace with hard delete if needed.
        /// </summary>
        [HttpDelete("{supplierId}")]
        public async Task<IActionResult> DeleteSupplier(int supplierId)
        {
            var response = new ResponseData();
            try
            {
                var supplier = await _context.Suppliers.FindAsync(supplierId);
                if (supplier == null)
                {
                    response.StatusCode = 404;
                    response.Message = "Supplier Not Found";
                    return StatusCode(response.StatusCode, response);
                }

                // soft-delete
                supplier.IsActive = false;
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.IsSuccess = true;
                response.Message = "Supplier deactivated";
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