using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegistationAndLogin.Domain.DTOs;
using RegistationAndLogin.Repositories;
using RegistationAndLogin.Service;
using System.Security.Claims;

namespace RegistationAndLogin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDto registerDto)
        {
            try
            {
                var response = await _authService.RegisterAsync(registerDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LogInDto logInDto)
        {
            try
            {
                var response = await _authService.LoginAsync(logInDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserDetailsAsync()
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId");
                if (userIdClaim == null|| !Guid.TryParse(userIdClaim.Value,out Guid userId))
                {
                    return Unauthorized("User Id not found in token");
                }

                var userDetails = await _authService.GetUserDetailsAsync(userId);
                if (userDetails == null)
                {
                    return NotFound("User not found");
                }
                return Ok(userDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }
        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserAsync( [FromBody] UserUpdateDto userUpdateDto)
        {
            try
            {
                var userIdClaim = User.FindFirst("UserId").Value;
                if(userIdClaim == null || !Guid.TryParse(userIdClaim,out Guid userId))
                {
                    return Unauthorized("User Id not found in token or does not match");
                }
                
                var updatedUser = await _authService.UpdateUserAsync(userId, userUpdateDto);
                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

