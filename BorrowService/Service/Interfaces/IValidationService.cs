namespace BorrowService.Service.Interfaces
{
    public interface IValidationService
    {
        Task<bool> ValidateUserExistsAsync(int userId);
        Task<bool> ValidateBookExistsAsync(int bookId);
        Task<bool> ValidateBookAvailabilityAsync(int bookId);
    }
}
