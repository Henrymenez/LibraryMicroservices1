using BookService.Models;
using Microsoft.EntityFrameworkCore;

namespace BookService.Data
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.ISBN).IsUnique();
                entity.Property(b => b.Title).HasMaxLength(500);
                entity.Property(b => b.Author).HasMaxLength(300);
                entity.Property(b => b.ISBN).HasMaxLength(50);
                entity.Property(b => b.Description).HasMaxLength(2000);
            });
        }
    }
}
