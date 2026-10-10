using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using CampusCafe.API.Models;
using CampusCafe.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SupportedOSPlatform("windows")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _repository;

        public ProductsController(ProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                var products = _repository.GetProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"MS Access Read Error: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult AddProduct([FromBody] Product product)
        {
            try
            {
                bool success = _repository.AddProduct(product);
                if (success) return Ok();
                return BadRequest("Failed to insert product into database.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"MS Access Insert Error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                bool success = _repository.DeleteProduct(id);
                if (success) return Ok();
                return NotFound($"Product with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"MS Access Delete Error: {ex.Message}");
            }
        }
    }
}