using System.Collections.Generic;
using CampusCafe.API.Models;
using CampusCafe.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly SupplierRepository _repo;

        public SuppliersController(SupplierRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult<List<SupplierDto>> Get()
        {
            return Ok(_repo.GetAllSuppliers());
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplierDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CompanyName))
            {
                return BadRequest("Company name is required.");
            }

            bool success = _repo.AddSupplier(dto);
            if (success)
            {
                return Ok();
            }

            return BadRequest("Failed to add supplier to database.");
        }

        [HttpGet("deliveries")]
        public ActionResult<List<DeliveryOverviewDto>> GetDeliveries()
        {
            return Ok(_repo.GetDeliveries());
        }
    }
}