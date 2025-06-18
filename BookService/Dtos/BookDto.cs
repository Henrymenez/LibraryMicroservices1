using System.ComponentModel.DataAnnotations;

namespace BookService.Dtos
{
    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class CreateBookDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string ISBN { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int TotalCopies { get; set; }
    }
}
