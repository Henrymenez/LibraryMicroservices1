using Newtonsoft.Json;

namespace BorrowService.Service.Implementation
{
    public class ValidationService
    {
        private readonly ILogger<ValidationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ValidationService(ILogger<ValidationService> logger, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> ValidateUserExistsAsync(int userId)
        {
            try
            {
                var userServiceUrl = _configuration["Services:UserService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{userServiceUrl}/api/users/{userId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ValidateBookExistsAsync(int bookId)
        {
            try
            {
                var bookServiceUrl = _configuration["Services:BookService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{bookServiceUrl}/api/books/{bookId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate book {BookId}", bookId);
                return false;
            }
        }

        public async Task<bool> ValidateBookAvailabilityAsync(int bookId)
        {
            try
            {
                var bookServiceUrl = _configuration["Services:BookService:BaseUrl"];
                var response = await _httpClient.GetAsync($"{bookServiceUrl}/api/books/{bookId}");

                if (response.IsSuccessStatusCode)
                {
                    var bookJson = await response.Content.ReadAsStringAsync();
                    var book = JsonConvert.DeserializeObject<dynamic>(bookJson);
                    return book?.availableCopies > 0;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check book availability {BookId}", bookId);
                return false;
            }
        }
    }
}
