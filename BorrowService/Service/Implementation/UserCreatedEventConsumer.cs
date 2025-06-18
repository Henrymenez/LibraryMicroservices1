using Confluent.Kafka;
using LibraryShared.Events;
using LibraryShared.Service.Implementation;

namespace BorrowService.Service.Implementation
{
    public class UserCreatedEventConsumer : KafkaConsumerBase<UserCreatedEvent>
    {
        public UserCreatedEventConsumer(
            ConsumerConfig config,
            ILogger<UserCreatedEventConsumer> logger)
            : base(config, "user-created", logger)
        {
        }

        protected override async Task ProcessMessageAsync(UserCreatedEvent message)
        {
            _logger.LogInformation("User created: {UserId} - {Name}", message.UserId, message.Name);
            await Task.CompletedTask;
        }
    }
}
