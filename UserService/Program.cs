
using Confluent.Kafka;
using LibraryShared.Service.Implementation;
using LibraryShared.Service.Interface;
using Microsoft.EntityFrameworkCore;
using UserService.Data;

namespace UserService
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
            builder.Services.AddDbContext<UserDbContext>(options =>
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
                // app.MapOpenApi();
           // }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            // Run database migrations
            //using (var scope = app.Services.CreateScope())
            //{
            //    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();
            //    context.Database.EnsureCreated();
            //}


            app.Run();
        }
    }
}
