using FlightManagementSystem.Models;
using FlightManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlightManagementSystem.API
{
    [Route("api/users")]
    [ApiController]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
           
            return Ok(users);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest(new { message = "User cannot be null." });
            }

            var newUser = await _userService.AddUserAsync(user);

            return Ok(newUser);
        }

        
        [HttpPut("{id}")]
        public async Task<ActionResult<User>> UpdateUser(string id, [FromBody] User updatedUser)
        {
            if (id != updatedUser.Id)
            {
                return BadRequest(new { message = "User ID in the route does not match the user object." });
            }

            try
            {
                var user = await _userService.UpdateUserAsync(updatedUser);
              
                return Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var deletedUser = await _userService.DeleteUserAsync(id);
               
                return Ok(new { message = $"User with ID {id} deleted successfully.", user = deletedUser });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
