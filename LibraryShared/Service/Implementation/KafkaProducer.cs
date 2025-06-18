using Confluent.Kafka;
using LibraryShared.Service.Interface;
using Newtonsoft.Json;

namespace LibraryShared.Service.Implementation
{
    public class KafkaProducer : IKafkaProducer, IDisposable
    {
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(ProducerConfig config)
        {
            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task ProduceAsync<T>(string topic, T message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);
            await _producer.ProduceAsync(topic, new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = serializedMessage
            });
        }

        public void Dispose()
        {
            _producer?.Dispose();
        }
    }
}
