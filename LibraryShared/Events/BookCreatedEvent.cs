﻿namespace LibraryShared.Events
{
    public class BookCreatedEvent
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
