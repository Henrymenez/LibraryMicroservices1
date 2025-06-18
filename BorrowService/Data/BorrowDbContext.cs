using BorrowService.Models;
using Microsoft.EntityFrameworkCore;

namespace BorrowService.Data
{
    public class BorrowDbContext : DbContext
    {
        public BorrowDbContext(DbContextOptions<BorrowDbContext> options) : base(options) { }

        public DbSet<Borrow> Borrows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Borrow>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.HasIndex(b => b.UserId);
                entity.HasIndex(b => b.BookId);
                entity.HasIndex(b => new { b.UserId, b.BookId, b.Status });
                entity.Property(b => b.Status).HasConversion<string>();
                entity.Property(b => b.Notes).HasMaxLength(1000);
            });
        }
    }
}
