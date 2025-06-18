
using BookService.Data;
using BookService.Service.BookAvailabilityService.cs;
using Confluent.Kafka;
using LibraryShared.Service.Implementation;
using LibraryShared.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace BookService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            // Add Entity Framework
            builder.Services.AddDbContext<BookDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Memory Cache
            builder.Services.AddMemoryCache();

            // Add Kafka Producer
            builder.Services.AddSingleton<IKafkaProducer>(provider =>
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = builder.Configuration.GetConnectionString("Kafka")
                };
                return new KafkaProducer(config);
            });

            // Add Kafka Consumers
            builder.Services.AddHostedService<BorrowEventConsumer>(provider =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = builder.Configuration.GetConnectionString("Kafka"),
                    GroupId = "book-service-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                var logger = provider.GetRequiredService<ILogger<BorrowEventConsumer>>();
                return new BorrowEventConsumer(config, provider, logger);
            });

            builder.Services.AddHostedService<ReturnEventConsumer>(provider =>
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = builder.Configuration.GetConnectionString("Kafka"),
                    GroupId = "book-service-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                var logger = provider.GetRequiredService<ILogger<ReturnEventConsumer>>();
                return new ReturnEventConsumer(config, provider, logger);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Run database migrations
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BookDbContext>();
                context.Database.EnsureCreated();
            }


            app.Run();
        }
    }
}
