using ContentService.DTOs;

namespace ContentService.ClientServices
{
    public class UserClientService : IUserClientService
    {
        private readonly HttpClient _httpClient;

        public UserClientService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("UserServiceClient");
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"users/{id}");
        }
       
        public async Task UpdateUserLastContentAsync(Guid userId, UserUpdateDto dto)
        {
            var response = await _httpClient.PatchAsJsonAsync($"api/users/{userId}/lastcontentupdated", dto);
            response.EnsureSuccessStatusCode();
        }
    }
}
