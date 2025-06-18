using BorrowService.Models;
using System.ComponentModel.DataAnnotations;

namespace BorrowService.DTOs
{
        public class CreateBorrowDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        public DateTime? DueDate { get; set; }

        public string Notes { get; set; } = string.Empty;
    }

    public class BorrowDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsOverdue => Status == BorrowStatus.Active && DateTime.UtcNow > DueDate;
    }

    public class ReturnBookDto
    {
        public string Notes { get; set; } = string.Empty;
    }
}
