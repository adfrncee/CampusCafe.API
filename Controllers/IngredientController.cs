using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using CampusCafe.API.Models;
using CampusCafe.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CampusCafe.API.Controllers
{
    [ApiController]
    [Route("api/ingredients")]
    [SupportedOSPlatform("windows")]
    public class IngredientController : ControllerBase
    {
        private readonly IngredientRepository _repo;

        public IngredientController(IngredientRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult<List<Ingredient>> Get()
        {
            return Ok(_repo.GetIngredients());
        }

        [HttpPost]
        public IActionResult Post([FromBody] IngredientItemDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.IngredientName))
            {
                return BadRequest("Ingredient name is required.");
            }

            bool success = _repo.AddIngredient(dto);
            if (success)
            {
                return Ok();
            }

            return BadRequest("Failed to add ingredient.");
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] double currentQuantity)
        {
            bool success = _repo.UpdateIngredientStock(id, currentQuantity);
            if (success) return Ok();
            return BadRequest("Failed to update ingredient stock.");
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            bool success = _repo.DeleteIngredient(id);
            if (success) return Ok();
            return BadRequest("Failed to delete ingredient.");
        }
    }
}