using BookService.Data;
using BookService.Dtos;
using BookService.Models;
using LibraryShared.Events;
using LibraryShared.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BookService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BookDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly ILogger<BooksController> _logger;

        public BooksController(
            BookDbContext context,
            IMemoryCache cache,
            IKafkaProducer kafkaProducer,
            ILogger<BooksController> logger)
        {
            _context = context;
            _cache = cache;
            _kafkaProducer = kafkaProducer;
            _logger = logger;
        }

        [HttpGet("getBooks")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            const string cacheKey = "all_books";

            if (!_cache.TryGetValue(cacheKey, out List<BookDto>? books))
            {
                var bookEntities = await _context.Books.ToListAsync();
                books = bookEntities.Select(b => new BookDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    ISBN = b.ISBN,
                    Description = b.Description,
                    TotalCopies = b.TotalCopies,
                    AvailableCopies = b.AvailableCopies,
                    CreatedAt = b.CreatedAt
                }).ToList();

                _cache.Set(cacheKey, books, TimeSpan.FromMinutes(15));
            }

            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            string cacheKey = $"book_{id}";

            if (!_cache.TryGetValue(cacheKey, out BookDto? book))
            {
                var bookEntity = await _context.Books.FindAsync(id);
                if (bookEntity == null)
                    return NotFound();

                book = new BookDto
                {
                    Id = bookEntity.Id,
                    Title = bookEntity.Title,
                    Author = bookEntity.Author,
                    ISBN = bookEntity.ISBN,
                    Description = bookEntity.Description,
                    TotalCopies = bookEntity.TotalCopies,
                    AvailableCopies = bookEntity.AvailableCopies,
                    CreatedAt = bookEntity.CreatedAt
                };
                _cache.Set(cacheKey, book, TimeSpan.FromMinutes(15));
            }

            return Ok(book);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromQuery] string? title, [FromQuery] string? author)
        {
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(title))
                query = query.Where(b => b.Title.Contains(title));

            if (!string.IsNullOrEmpty(author))
                query = query.Where(b => b.Author.Contains(author));

            var books = await query.Select(b => new BookDto
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                Description = b.Description,
                TotalCopies = b.TotalCopies,
                AvailableCopies = b.AvailableCopies,
                CreatedAt = b.CreatedAt
            }).ToListAsync();

            return Ok(books);
        }

        [HttpPost("createBook")]
        public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
        {
            var book = new Book
            {
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                ISBN = createBookDto.ISBN,
                Description = createBookDto.Description,
                TotalCopies = createBookDto.TotalCopies,
                AvailableCopies = createBookDto.TotalCopies,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Publish event to Kafka
            var bookCreatedEvent = new BookCreatedEvent
            {
                BookId = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                TotalCopies = book.TotalCopies,
                CreatedAt = book.CreatedAt
            };

            await _kafkaProducer.ProduceAsync("book-created", bookCreatedEvent);

            // Clear cache
            _cache.Remove("all_books");

            var bookDto = new BookDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                ISBN = book.ISBN,
                Description = book.Description,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                CreatedAt = book.CreatedAt
            };

            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, bookDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, CreateBookDto updateBookDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound();

            var copyDifference = updateBookDto.TotalCopies - book.TotalCopies;

            book.Title = updateBookDto.Title;
            book.Author = updateBookDto.Author;
            book.ISBN = updateBookDto.ISBN;
            book.Description = updateBookDto.Description;
            book.TotalCopies = updateBookDto.TotalCopies;
            book.AvailableCopies += copyDifference;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Clear cache
            _cache.Remove($"book_{id}");
            return Ok(book);
        }
    }
}
