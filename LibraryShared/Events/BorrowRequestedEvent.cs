namespace LibraryShared.Events
{
    public class BorrowRequestedEvent
    {
        public int BorrowId { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
