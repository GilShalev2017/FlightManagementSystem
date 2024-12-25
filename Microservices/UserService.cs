using FlightManagementSystem.Infrastructure;
using FlightManagementSystem.Models;
using MongoDB.Bson;

namespace FlightManagementSystem.Services
{
    public interface IUserService
    {
        //Managing Users
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(string id);
        Task<IEnumerable<User>> GetAllUsersAsync(string? name = null);
        Task<User> UpdateUserAsync(User user);
        Task<User> DeleteUserAsync(string id);
        //Managing AlertPreferences
        Task<User?> AddAlertPreferenceAsync(string userId, UserAlertPreference preference);
        Task<User?> UpdateAlertPreferenceAsync(string userId, string preferenceId, UserAlertPreference preference);
        Task<User?> DeleteAlertPreferenceAsync(string userId, string preferenceId);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        #region Users 
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AddUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            // Prevent preferences from being added during user creation
            user.AlertPreferences.Clear();

            return await _userRepository.AddUserAsync(user);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }

            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(string? name = null)
        {
            var users = await _userRepository.GetAllUsersAsync();

            if (!string.IsNullOrEmpty(name))
            {
                return users.Where(u => u.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            return users;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User cannot be null.");
            }

            // Fetch the existing user
            var existingUser = await _userRepository.GetUserByIdAsync(user.Id!);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} was not found.");
            }

            // Preserve the existing AlertPreferences
            user.AlertPreferences = existingUser.AlertPreferences;

            // Proceed with updating the user
            var updatedUser = await _userRepository.UpdateUserAsync(user);
            if (updatedUser == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} could not be updated.");
            }

            return updatedUser;
        }


        public async Task<User> DeleteUserAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(id));
            }

            var deletedUser = await _userRepository.DeleteUserAsync(id);

            if (deletedUser == null)
            {
                throw new InvalidOperationException($"User with ID {id} was not found.");
            }

            return deletedUser;
        }
        #endregion Users

        #region AlertPreferences
        public async Task<User?> AddAlertPreferenceAsync(string userId, UserAlertPreference preference)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            // Assign a unique ID if not already set
            if (string.IsNullOrEmpty(preference.PreferenceId))
            {
                preference.PreferenceId = ObjectId.GenerateNewId().ToString();
            }

            user.AlertPreferences.Add(preference);
            await _userRepository.UpdateUserAsync(user);
            return user;
        }

        public async Task<User?> UpdateAlertPreferenceAsync(string userId, string preferenceId, UserAlertPreference updatedPreference)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var existingPreference = user.AlertPreferences.FirstOrDefault(p => p.PreferenceId == preferenceId);
            if (existingPreference == null)
            {
                return null;
            }

            // Update fields
            existingPreference.Destination = updatedPreference.Destination;
            existingPreference.MaxPrice = updatedPreference.MaxPrice;
            existingPreference.Currency = updatedPreference.Currency;

            await _userRepository.UpdateUserAsync(user);
            return user;
        }

        public async Task<User?> DeleteAlertPreferenceAsync(string userId, string preferenceId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var preference = user.AlertPreferences.FirstOrDefault(p => p.PreferenceId == preferenceId);
            if (preference == null)
            {
                return null;
            }

            user.AlertPreferences.Remove(preference);
            await _userRepository.UpdateUserAsync(user);
            return user;
        }
        #endregion AlertPreferences
    }

}
