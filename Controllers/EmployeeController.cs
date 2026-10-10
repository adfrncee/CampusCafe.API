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
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repo;

        public EmployeeController(EmployeeRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public ActionResult<List<Employee>> Get()
        {
            return Ok(_repo.GetAllEmployees());
        }

        [HttpPost]
        public IActionResult Post([FromBody] Employee dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Employee name is required.");
            }

            try
            {
                bool success = _repo.AddEmployee(dto);
                if (success) return Ok();

                return BadRequest("Failed to add employee.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Database Error: {ex.Message}");
            }
        }
    }
}