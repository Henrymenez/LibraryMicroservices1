
using BorrowService.Data;
using BorrowService.Service.Implementation;
using BorrowService.Service.Interfaces;
using Confluent.Kafka;
using LibraryShared.Service.Implementation;
using LibraryShared.Service.Interface;
using Microsoft.EntityFrameworkCore;

namespace BorrowService
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
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            // Add Entity Framework
            builder.Services.AddDbContext<BorrowDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Memory Cache
            builder.Services.AddMemoryCache();

            // Add HttpClient for service communication
           builder.Services.AddHttpClient<IValidationService, ValidationService>();
            builder.Services.AddScoped<IValidationService, ValidationService>();

            // Add Kafka Producer
            //builder.Services.AddSingleton<IKafkaProducer>(provider =>
            //{
            //    var config = new ProducerConfig
            //    {
            //        BootstrapServers = builder.Configuration.GetConnectionString("Kafka")
            //    };
            //    return new KafkaProducer(config);
            //});

            // Add Kafka Consumers
            //builder.Services.AddHostedService<UserCreatedEventConsumer>(provider =>
            //{
            //    var config = new ConsumerConfig
            //    {
            //        BootstrapServers = builder.Configuration.GetConnectionString("Kafka"),
            //        GroupId = "borrow-service-group",
            //        AutoOffsetReset = AutoOffsetReset.Earliest
            //    };
            //    var logger = provider.GetRequiredService<ILogger<UserCreatedEventConsumer>>();
            //    return new UserCreatedEventConsumer(config, logger);
            //});

            //builder.Services.AddHostedService<BookCreatedEventConsumer>(provider =>
            //{
            //    var config = new ConsumerConfig
            //    {
            //        BootstrapServers = builder.Configuration.GetConnectionString("Kafka"),
            //        GroupId = "borrow-service-group",
            //        AutoOffsetReset = AutoOffsetReset.Earliest
            //    };
            //    var logger = provider.GetRequiredService<ILogger<BookCreatedEventConsumer>>();
            //    return new BookCreatedEventConsumer(config, logger);
            //});


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //    app.MapOpenApi();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            // Run database migrations
            //using (var scope = app.Services.CreateScope())
            //{
            //    var context = scope.ServiceProvider.GetRequiredService<BorrowDbContext>();
            //    context.Database.EnsureCreated();
            //}
            app.Run();
        }
    }
}
