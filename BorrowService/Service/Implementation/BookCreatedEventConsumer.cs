using Confluent.Kafka;
using LibraryShared.Events;
using LibraryShared.Service.Implementation;

namespace BorrowService.Service.Implementation
{

    public class BookCreatedEventConsumer : KafkaConsumerBase<BookCreatedEvent>
    {
        public BookCreatedEventConsumer(
            ConsumerConfig config,
            ILogger<BookCreatedEventConsumer> logger)
            : base(config, "book-created", logger)
        {
        }

        protected override async Task ProcessMessageAsync(BookCreatedEvent message)
        {
            _logger.LogInformation("Book created: {BookId} - {Title}", message.BookId, message.Title);
            await Task.CompletedTask;
        }
    }
}
