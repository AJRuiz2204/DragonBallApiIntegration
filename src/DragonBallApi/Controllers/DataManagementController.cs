using DragonBallApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DragonBallApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataManagementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DataManagementController> _logger;

        public DataManagementController(ApplicationDbContext context, ILogger<DataManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // DELETE: api/datamanagement/clear
        [HttpDelete("clear")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearDatabaseData()
        {
            _logger.LogWarning("Attempting to clear all Character and Transformation data from the database.");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Borrar datos existentes (¡Orden inverso por Foreign Key!)
                    _logger.LogInformation("Deleting existing transformations...");
                    int transformationsDeleted = await _context.Transformations.ExecuteDeleteAsync();
                    _logger.LogInformation("{Count} transformations deleted.", transformationsDeleted);

                    _logger.LogInformation("Deleting existing characters...");
                    int charactersDeleted = await _context.Characters.ExecuteDeleteAsync();
                    _logger.LogInformation("{Count} characters deleted.", charactersDeleted);

                    await transaction.CommitAsync();
                    _logger.LogInformation("Database cleared successfully.");
                    return Ok("All character and transformation data cleared successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while clearing database data. Transaction rolled back.");
                    return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while clearing the database.");
                }
            });
        }
    }
}