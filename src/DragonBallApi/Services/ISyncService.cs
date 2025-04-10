namespace DragonBallApi.Services
{
    public interface ISyncService
    {
        Task<(bool Success, string Message)> SynchronizeCharactersAsync();
    }
}