using BookService.Data;
using Confluent.Kafka;
using LibraryShared.Events;
using LibraryShared.Service.Implementation;
using Microsoft.Extensions.Caching.Memory;

namespace BookService.Service.BookAvailabilityService.cs
{
    public class BorrowEventConsumer : KafkaConsumerBase<BorrowRequestedEvent>
    {
        private readonly IServiceProvider _serviceProvider;

        public BorrowEventConsumer(
            ConsumerConfig config,
            IServiceProvider serviceProvider,
            ILogger<BorrowEventConsumer> logger)
            : base(config, "borrow-requested", logger)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ProcessMessageAsync(BorrowRequestedEvent message)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

            var book = await context.Books.FindAsync(message.BookId);
            if (book != null && book.AvailableCopies > 0)
            {
                book.AvailableCopies--;
                await context.SaveChangesAsync();

                // Clear cache
                cache.Remove($"book_{book.Id}");
                cache.Remove("all_books");

                _logger.LogInformation("Reduced available copies for book {BookId}", message.BookId);
            }
        }
    }
}
