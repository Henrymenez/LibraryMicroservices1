using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LibraryShared.Service.Implementation
{
    public abstract class KafkaConsumerBase<T> : BackgroundService
    {
        protected readonly IConsumer<string, string> _consumer;
        protected readonly ILogger _logger;
        protected readonly string _topic;

        protected KafkaConsumerBase(ConsumerConfig config, string topic, ILogger logger)
        {
            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _topic = topic;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    if (consumeResult?.Message != null)
                    {
                        var message = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(consumeResult.Message.Value);
                        if (message != null)
                        {
                            await ProcessMessageAsync(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message from topic {Topic}", _topic);
                }
            }
        }

        protected abstract Task ProcessMessageAsync(T message);

        public override void Dispose()
        {
            _consumer?.Dispose();
            base.Dispose();
        }

    }
}
