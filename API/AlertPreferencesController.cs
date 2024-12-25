using FlightManagementSystem.Models;
using FlightManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlightManagementSystem.API
{
    [Route("api/users/{userId}/preferences")]
    [ApiController]
    public class AlertPreferencesController : ControllerBase
    {
        private readonly IUserService _userService;

        public AlertPreferencesController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddAlertPreference(string userId, [FromBody] UserAlertPreference newPreference)
        {
            if (newPreference == null)
            {
                return BadRequest("Preference cannot be null.");
            }

            var updatedUser = await _userService.AddAlertPreferenceAsync(userId, newPreference);
            if (updatedUser == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            return Ok(updatedUser);
        }

        [HttpPut("{preferenceId}")]
        public async Task<IActionResult> UpdateAlertPreference(string userId, string preferenceId, [FromBody] UserAlertPreference updatedPreference)
        {
            if (updatedPreference == null)
            {
                return BadRequest("Preference cannot be null.");
            }

            var user = await _userService.UpdateAlertPreferenceAsync(userId, preferenceId, updatedPreference);
            if (user == null)
            {
                return NotFound($"User with ID {userId} or preference with ID {preferenceId} not found.");
            }

            return Ok(user);
        }

        [HttpDelete("{preferenceId}")]
        public async Task<IActionResult> DeleteAlertPreference(string userId, string preferenceId)
        {
            var user = await _userService.DeleteAlertPreferenceAsync(userId, preferenceId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} or preference with ID {preferenceId} not found.");
            }

            return Ok(user);
        }
    }

}
