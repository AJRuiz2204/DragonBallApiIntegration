using DragonBallApi.Data;
using DragonBallApi.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DragonBallApi.Services
{
    public class SyncService : ISyncService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDragonBallApiService _dragonBallApiService;
        private readonly ILogger<SyncService> _logger;

        public SyncService(ApplicationDbContext context, IDragonBallApiService dragonBallApiService, ILogger<SyncService> logger)
        {
            _context = context;
            _dragonBallApiService = dragonBallApiService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> SynchronizeCharactersAsync()
        {
            _logger.LogInformation("Starting synchronization process...");

           
            if (await _context.Characters.AnyAsync() || await _context.Transformations.AnyAsync())
            {
                _logger.LogWarning("Synchronization aborted. Database is not empty. Please clear data first.");
                return (false, "Database already contains data. Please clear existing characters and transformations before syncing.");
            }

            List<Character> charactersToSave = new List<Character>();
            List<Transformation> transformationsToSave = new List<Transformation>();
            int currentPage = 1;
            int totalPages = 1;
            int saiyanCount = 0;
            int transformationCount = 0;

            try
            {
                do
                {
                    _logger.LogInformation("Fetching page {CurrentPage} of characters from external API.", currentPage);
                    var apiResponse = await _dragonBallApiService.GetAllCharactersAsync(page: currentPage, limit: 50);

                    if (apiResponse == null)
                    {
                        _logger.LogError("Failed to fetch page {CurrentPage} from external API.", currentPage);
                        return (false, $"Error fetching data from external API on page {currentPage}.");
                    }

                    if (currentPage == 1)
                    {
                        totalPages = apiResponse.Meta.TotalPages;
                         _logger.LogInformation("Total pages to fetch: {TotalPages}", totalPages);
                    }

                   
                    var saiyans = apiResponse.Items
                        .Where(c => !string.IsNullOrEmpty(c.Race) && c.Race.Equals("Saiyan", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                     _logger.LogDebug("Found {SaiyanCount} Saiyans on page {CurrentPage}.", saiyans.Count, currentPage);

                    foreach (var externalChar in saiyans)
                    {
                         saiyanCount++;
                        var characterEntity = new Character
                        {
                            Id = externalChar.Id,
                            Name = externalChar.Name,
                            Ki = externalChar.Ki,
                            Race = externalChar.Race,
                            Gender = externalChar.Gender,
                            Description = externalChar.Description,
                            Affiliation = externalChar.Affiliation
                        };
                        charactersToSave.Add(characterEntity);

                       
                        if (!string.IsNullOrEmpty(externalChar.Affiliation) && externalChar.Affiliation.Equals("Z Fighter", StringComparison.OrdinalIgnoreCase))
                        {
                           
                            _logger.LogDebug("Fetching details for Z Fighter Saiyan: {CharacterName} (ID: {CharacterId})", externalChar.Name, externalChar.Id);
                            var detailedCharacter = await _dragonBallApiService.GetCharacterByIdAsync(externalChar.Id);

                            if (detailedCharacter?.Transformations != null && detailedCharacter.Transformations.Any())
                            {
                                 _logger.LogDebug("Found {TransformationCount} transformations for {CharacterName}.", detailedCharacter.Transformations.Count, detailedCharacter.Name);
                                foreach (var externalTrans in detailedCharacter.Transformations)
                                {
                                     transformationCount++;
                                    transformationsToSave.Add(new Transformation
                                    {
                                        Id = externalTrans.Id,
                                        Name = externalTrans.Name,
                                        Ki = externalTrans.Ki,
                                        CharacterId = externalChar.Id
                                    });
                                }
                            }
                             else
                             {
                                 _logger.LogDebug("No transformations found for Z Fighter Saiyan: {CharacterName}", externalChar.Name);
                             }
                        }
                    }

                    currentPage++;

                } while (currentPage <= totalPages);


                 _logger.LogInformation("Finished fetching. Total Saiyans processed: {SaiyanCount}. Total Transformations for Z Fighters: {TransformationCount}.", saiyanCount, transformationCount);

               
                if (charactersToSave.Any())
                {
                    _logger.LogInformation("Saving {CharacterCount} Saiyan characters to the database...", charactersToSave.Count);
                    await _context.Characters.AddRangeAsync(charactersToSave);
                }
                if (transformationsToSave.Any())
                {
                     _logger.LogInformation("Saving {TransformationCount} transformations to the database...", transformationsToSave.Count);
                    await _context.Transformations.AddRangeAsync(transformationsToSave);
                }

                if (charactersToSave.Any() || transformationsToSave.Any())
                {
                    await _context.SaveChangesAsync();
                     _logger.LogInformation("Database save successful.");
                }
                 else
                {
                     _logger.LogInformation("No new data to save.");
                     return (true, "Synchronization complete. No Saiyans or relevant transformations found to save.");
                }


                _logger.LogInformation("Synchronization process completed successfully.");
                return (true, $"Synchronization successful. Saved {charactersToSave.Count} Saiyans and {transformationsToSave.Count} transformations.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during synchronization.");
               
                return (false, $"An error occurred during synchronization: {ex.Message}");
            }
        }
    }
}