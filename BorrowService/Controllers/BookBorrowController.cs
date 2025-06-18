using BorrowService.Data;
using BorrowService.DTOs;
using BorrowService.Models;
using BorrowService.Service.Interfaces;
using LibraryShared.Events;
using LibraryShared.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BorrowService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookBorrowController : Controller
    {
      
            private readonly BorrowDbContext _context;
            private readonly IMemoryCache _cache;
            private readonly IKafkaProducer _kafkaProducer;
            private readonly IValidationService _validationService;
            private readonly ILogger<BookBorrowController> _logger;

            public BookBorrowController(
                BorrowDbContext context,
                IMemoryCache cache,
                IKafkaProducer kafkaProducer,
                IValidationService validationService,
                ILogger<BookBorrowController> logger)
            {
                _context = context;
                _cache = cache;
                _kafkaProducer = kafkaProducer;
                _validationService = validationService;
                _logger = logger;
            }

            [HttpGet]
            public async Task<ActionResult<IEnumerable<BorrowDto>>> GetBorrows()
            {
                const string cacheKey = "all_borrows";

                if (!_cache.TryGetValue(cacheKey, out List<BorrowDto>? borrows))
                {
                    var borrowEntities = await _context.Borrows.ToListAsync();
                    borrows = borrowEntities.Select(b => new BorrowDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        BookId = b.BookId,
                        BorrowDate = b.BorrowDate,
                        DueDate = b.DueDate,
                        ReturnDate = b.ReturnDate,
                        Status = b.Status,
                        Notes = b.Notes,
                        CreatedAt = b.CreatedAt
                    }).ToList();

                    _cache.Set(cacheKey, borrows, TimeSpan.FromMinutes(5));
                }

                return Ok(borrows);
            }

            [HttpGet("{id}")]
            public async Task<ActionResult<BorrowDto>> GetBorrow(int id)
            {
                string cacheKey = $"borrow_{id}";

                if (!_cache.TryGetValue(cacheKey, out BorrowDto? borrow))
                {
                    var borrowEntity = await _context.Borrows.FindAsync(id);
                    if (borrowEntity == null)
                        return NotFound();

                    borrow = new BorrowDto
                    {
                        Id = borrowEntity.Id,
                        UserId = borrowEntity.UserId,
                        BookId = borrowEntity.BookId,
                        BorrowDate = borrowEntity.BorrowDate,
                        DueDate = borrowEntity.DueDate,
                        ReturnDate = borrowEntity.ReturnDate,
                        Status = borrowEntity.Status,
                        Notes = borrowEntity.Notes,
                        CreatedAt = borrowEntity.CreatedAt
                    };

                    _cache.Set(cacheKey, borrow, TimeSpan.FromMinutes(5));
                }

                return Ok(borrow);
            }

            [HttpGet("user/{userId}")]
            public async Task<ActionResult<IEnumerable<BorrowDto>>> GetBorrowsByUser(int userId)
            {
                var borrows = await _context.Borrows
                    .Where(b => b.UserId == userId)
                    .Select(b => new BorrowDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        BookId = b.BookId,
                        BorrowDate = b.BorrowDate,
                        DueDate = b.DueDate,
                        ReturnDate = b.ReturnDate,
                        Status = b.Status,
                        Notes = b.Notes,
                        CreatedAt = b.CreatedAt
                    }).ToListAsync();

                return Ok(borrows);
            }

            [HttpGet("overdue")]
            public async Task<ActionResult<IEnumerable<BorrowDto>>> GetOverdueBorrows()
            {
                var today = DateTime.UtcNow.Date;
                var overdueBorrows = await _context.Borrows
                    .Where(b => b.Status == BorrowStatus.Active && b.DueDate < today)
                    .Select(b => new BorrowDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        BookId = b.BookId,
                        BorrowDate = b.BorrowDate,
                        DueDate = b.DueDate,
                        ReturnDate = b.ReturnDate,
                        Status = b.Status,
                        Notes = b.Notes,
                        CreatedAt = b.CreatedAt
                    }).ToListAsync();

                return Ok(overdueBorrows);
            }

            [HttpPost]
            public async Task<ActionResult<BorrowDto>> CreateBorrow(CreateBorrowDto createBorrowDto)
            {
                // Validate user exists
                if (!await _validationService.ValidateUserExistsAsync(createBorrowDto.UserId))
                {
                    return BadRequest("User not found");
                }

                // Validate book exists and is available
                if (!await _validationService.ValidateBookExistsAsync(createBorrowDto.BookId))
                {
                    return BadRequest("Book not found");
                }

                if (!await _validationService.ValidateBookAvailabilityAsync(createBorrowDto.BookId))
                {
                    return BadRequest("Book is not available for borrowing");
                }

                // Check if user already has this book borrowed
                var existingBorrow = await _context.Borrows
                    .FirstOrDefaultAsync(b => b.UserId == createBorrowDto.UserId &&
                                             b.BookId == createBorrowDto.BookId &&
                                             b.Status == BorrowStatus.Active);

                if (existingBorrow != null)
                {
                    return BadRequest("User already has this book borrowed");
                }

                var dueDate = createBorrowDto.DueDate ?? DateTime.UtcNow.AddDays(14); // Default 2 weeks

                var borrow = new Borrow
                {
                    UserId = createBorrowDto.UserId,
                    BookId = createBorrowDto.BookId,
                    BorrowDate = DateTime.UtcNow,
                    DueDate = dueDate,
                    Status = BorrowStatus.Active,
                    Notes = createBorrowDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Borrows.Add(borrow);
                await _context.SaveChangesAsync();

                // Publish event to Kafka
                var borrowRequestedEvent = new BorrowRequestedEvent
                {
                    BorrowId = borrow.Id,
                    UserId = borrow.UserId,
                    BookId = borrow.BookId,
                    BorrowDate = borrow.BorrowDate,
                    DueDate = borrow.DueDate
                };

                await _kafkaProducer.ProduceAsync("borrow-requested", borrowRequestedEvent);

                // Clear cache
                _cache.Remove("all_borrows");

                var borrowDto = new BorrowDto
                {
                    Id = borrow.Id,
                    UserId = borrow.UserId,
                    BookId = borrow.BookId,
                    BorrowDate = borrow.BorrowDate,
                    DueDate = borrow.DueDate,
                    ReturnDate = borrow.ReturnDate,
                    Status = borrow.Status,
                    Notes = borrow.Notes,
                    CreatedAt = borrow.CreatedAt
                };

                return CreatedAtAction(nameof(GetBorrow), new { id = borrow.Id }, borrowDto);
            }

            [HttpPost("{id}/return")]
            public async Task<IActionResult> ReturnBook(int id, ReturnBookDto returnBookDto)
            {
                var borrow = await _context.Borrows.FindAsync(id);
                if (borrow == null)
                    return NotFound();

                if (borrow.Status != BorrowStatus.Active)
                    return BadRequest("Book is already returned");

                borrow.ReturnDate = DateTime.UtcNow;
                borrow.Status = BorrowStatus.Returned;
                borrow.Notes = returnBookDto.Notes;
                borrow.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Publish event to Kafka
                var borrowReturnedEvent = new BorrowReturnedEvent
                {
                    BorrowId = borrow.Id,
                    UserId = borrow.UserId,
                    BookId = borrow.BookId,
                    ReturnDate = borrow.ReturnDate.Value
                };

                await _kafkaProducer.ProduceAsync("borrow-returned", borrowReturnedEvent);

                // Clear cache
                _cache.Remove($"borrow_{id}");
                _cache.Remove("all_borrows");

                return NoContent();
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteBorrow(int id)
            {
                var borrow = await _context.Borrows.FindAsync(id);
                if (borrow == null)
                    return NotFound();

                _context.Borrows.Remove(borrow);
                await _context.SaveChangesAsync();

                // Clear cache
                _cache.Remove($"borrow_{id}");
                _cache.Remove("all_borrows");

                return NoContent();
            }
        }
}
