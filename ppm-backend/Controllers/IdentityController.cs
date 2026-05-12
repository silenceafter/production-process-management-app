using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PpmBackend.Models.DTOs;
using PpmBackend.Models.Identity;
using System.Security.Claims;

namespace PpmBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IdentityController : ControllerBase
    {
        [HttpGet("hash")]
        public IActionResult GetTestHash()
        {
            try
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                var hash = passwordHasher.HashPassword(null, "TempP@ss123!");
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}