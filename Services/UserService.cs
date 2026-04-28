using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Services
{
    /// <summary>
    /// In-memory implementation of IUserService.
    /// In a real-world project, this would interact with a database via a repository.
    /// </summary>
    public class UserService : IUserService
    {
        // Thread-safe in-memory store using a Dictionary
        private static readonly Dictionary<int, User> _users = new();
        private static int _nextId = 1;
        private static readonly object _lock = new();

        // Seed some initial data
        static UserService()
        {
            var seed = new List<User>
            {
                new() { Id = 1, Name = "Alice Johnson",  Email = "alice@techhive.com",  Role = "Admin",    CreatedAt = DateTime.UtcNow.AddDays(-30) },
                new() { Id = 2, Name = "Bob Martinez",   Email = "bob@techhive.com",    Role = "Employee", CreatedAt = DateTime.UtcNow.AddDays(-20) },
                new() { Id = 3, Name = "Carol Williams", Email = "carol@techhive.com",  Role = "Manager",  CreatedAt = DateTime.UtcNow.AddDays(-10) },
            };

            foreach (var user in seed)
                _users[user.Id] = user;

            _nextId = seed.Count + 1;
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            lock (_lock)
            {
                return Task.FromResult<IEnumerable<User>>(_users.Values.ToList());
            }
        }

        public Task<User?> GetUserByIdAsync(int id)
        {
            lock (_lock)
            {
                _users.TryGetValue(id, out var user);
                return Task.FromResult(user);
            }
        }

        public Task<User> CreateUserAsync(CreateUserDto dto)
        {
            lock (_lock)
            {
                var user = new User
                {
                    Id        = _nextId++,
                    Name      = dto.Name.Trim(),
                    Email     = dto.Email.Trim().ToLowerInvariant(),
                    Role      = dto.Role.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _users[user.Id] = user;
                return Task.FromResult(user);
            }
        }

        public Task<User?> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            lock (_lock)
            {
                if (!_users.TryGetValue(id, out var user))
                    return Task.FromResult<User?>(null);

                user.Name      = dto.Name.Trim();
                user.Email     = dto.Email.Trim().ToLowerInvariant();
                user.Role      = dto.Role.Trim();
                user.UpdatedAt = DateTime.UtcNow;

                return Task.FromResult<User?>(user);
            }
        }

        public Task<bool> DeleteUserAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_users.Remove(id));
            }
        }

        public Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            lock (_lock)
            {
                var normalised = email.Trim().ToLowerInvariant();
                var exists = _users.Values.Any(u =>
                    u.Email == normalised && (excludeId == null || u.Id != excludeId));
                return Task.FromResult(exists);
            }
        }
    }
}
