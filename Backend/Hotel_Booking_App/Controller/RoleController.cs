using Hotel_Booking_App.Interface;
using Hotel_Booking_App.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Booking_App.Controller
{
    [ApiController]
    [Route("api/roles")]
    [EnableCors("AllowAllOrigins")]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("update-Role")]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleDto dto)
        {
            var success = await _roleService.UpdateUserRoleAsync(dto);
            if (!success)
                return NotFound("User not found");

            return Ok("User role updated successfully");
        }
    }
}
