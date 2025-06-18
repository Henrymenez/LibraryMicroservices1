using System.ComponentModel.DataAnnotations;

namespace BorrowService.Models
{
    public class Borrow
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        public DateTime BorrowDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; }

        public DateTime? ReturnDate { get; set; }

        public BorrowStatus Status { get; set; } = BorrowStatus.Active;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
    public enum BorrowStatus
    {
        Active = 1,
        Returned,
        Overdue
    }
}
