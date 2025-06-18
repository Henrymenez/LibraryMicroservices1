using LibraryShared.Events;
using LibraryShared.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers
{
        [ApiController]
        [Route("api/[controller]")]
        public class UsersController : ControllerBase
        {
            private readonly UserDbContext _context;
            private readonly IMemoryCache _cache;
            private readonly IKafkaProducer _kafkaProducer;
            private readonly ILogger<UsersController> _logger;

            public UsersController(
                UserDbContext context,
                IMemoryCache cache,
                IKafkaProducer kafkaProducer,
                ILogger<UsersController> logger)
            {
                _context = context;
                _cache = cache;
                _kafkaProducer = kafkaProducer;
                _logger = logger;
            }

            [HttpGet("getUsers")]
            public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
            {
                const string cacheKey = "all_users";

                if (!_cache.TryGetValue(cacheKey, out List<UserDto>? users))
                {
                    var userEntities = await _context.Users.ToListAsync();
                    users = userEntities.Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Name = u.Name,
                        Phone = u.Phone,
                        CreatedAt = u.CreatedAt
                    }).ToList();

                    _cache.Set(cacheKey, users, TimeSpan.FromMinutes(10));
                }

                return Ok(users);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<UserDto>> GetUser(int id)
            {
                string cacheKey = $"user_{id}";

                if (!_cache.TryGetValue(cacheKey, out UserDto? user))
                {
                    var userEntity = await _context.Users.FindAsync(id);
                    if (userEntity == null)
                        return NotFound();

                    user = new UserDto
                    {
                        Id = userEntity.Id,
                        Email = userEntity.Email,
                        Name = userEntity.Name,
                        Phone = userEntity.Phone,
                        CreatedAt = userEntity.CreatedAt
                    };

                    _cache.Set(cacheKey, user, TimeSpan.FromMinutes(10));
                }

                return Ok(user);
            }

            [HttpPost("createUser")]
            public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
            {
                var user = new User
                {
                    Email = createUserDto.Email,
                    Name = createUserDto.Name,
                    Phone = createUserDto.Phone,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Publish event to Kafka
                var userCreatedEvent = new UserCreatedEvent
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    CreatedAt = user.CreatedAt
                };

                await _kafkaProducer.ProduceAsync("user-created", userCreatedEvent);

                // Clear cache
                _cache.Remove("all_users");

                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt
                };

                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateUser(int id, CreateUserDto updateUserDto)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound();

                user.Email = updateUserDto.Email;
                user.Name = updateUserDto.Name;
                user.Phone = updateUserDto.Phone;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Clear cache
                _cache.Remove($"user_{id}");
                _cache.Remove("all_users");

                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteUser(int id)
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound();

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                // Clear cache
                _cache.Remove($"user_{id}");
                _cache.Remove("all_users");

                return NoContent();
            }
        }
}
