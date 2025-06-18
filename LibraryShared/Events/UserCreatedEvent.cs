namespace LibraryShared.Events
{
    public class UserCreatedEvent
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
