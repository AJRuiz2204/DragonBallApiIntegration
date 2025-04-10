using DragonBallApi.Models.ExternalApi;

namespace DragonBallApi.Services
{
    public interface IDragonBallApiService
    {
        Task<ApiResponse<ExternalCharacter>?> GetAllCharactersAsync(int page = 1, int limit = 10);
        Task<ExternalCharacter?> GetCharacterByIdAsync(int id);
    }
}