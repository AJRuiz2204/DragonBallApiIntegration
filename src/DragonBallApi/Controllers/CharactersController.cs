using DragonBallApi.Data;
using DragonBallApi.Models.Database;
using DragonBallApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DragonBallApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISyncService _syncService;
        private readonly ILogger<CharactersController> _logger;


        public CharactersController(ApplicationDbContext context, ISyncService syncService, ILogger<CharactersController> logger)
        {
            _context = context;
            _syncService = syncService;
            _logger = logger;
        }


        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Character>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharacters()
        {
            _logger.LogInformation("Getting all characters from DB");

            return await _context.Characters
                                 .Include(c => c.Transformations)
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Character), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Character>> GetCharacter(int id)
        {
            _logger.LogInformation("Getting character with ID: {CharacterId}", id);
            var character = await _context.Characters
                                          .Include(c => c.Transformations)
                                          .FirstOrDefaultAsync(c => c.Id == id);

            if (character == null)
            {
                _logger.LogWarning("Character with ID: {CharacterId} not found.", id);
                return NotFound();
            }

            return character;
        }


        [HttpGet("byName/{name}")]
        [ProducesResponseType(typeof(IEnumerable<Character>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharactersByName(string name)
        {
            _logger.LogInformation("Getting characters from LOCAL DB with name containing: {Name}", name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name parameter cannot be empty.");
            }

            var characters = await _context.Characters
                                 .Include(c => c.Transformations) // Incluye transformaciones desde la DB
                                 .Where(c => EF.Functions.Like(c.Name, $"%{name}%"))
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();

            if (!characters.Any())
            {
                _logger.LogWarning("No characters found in LOCAL DB with name containing: {Name}", name);
                return NotFound($"No characters found matching the name '{name}'.");
            }

            // Mapear a DTO antes de devolver
            var characterDtos = characters.Select(c => new Character
            {
                Id = c.Id,
                Name = c.Name,
                Ki = c.Ki,
                Race = c.Race,
                Gender = c.Gender,
                Description = c.Description,
                Affiliation = c.Affiliation,
                Transformations = c.Transformations.Select(t => new Transformation
                {
                    Id = t.Id,
                    Name = t.Name,
                    Ki = t.Ki
                }).ToList()
            }).ToList();

            return Ok(characterDtos);
        }


        [HttpGet("byAffiliation/{affiliation}")]
        [ProducesResponseType(typeof(IEnumerable<Character>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Character>>> GetCharactersByAffiliation(string affiliation)
        {
            _logger.LogInformation("Getting characters from LOCAL DB with affiliation: {Affiliation}", affiliation);
            if (string.IsNullOrWhiteSpace(affiliation))
            {
                return BadRequest("Affiliation parameter cannot be empty.");
            }

            var characters = await _context.Characters
                                .Include(c => c.Transformations) // Incluye transformaciones desde la DB
                                .Where(c => c.Affiliation.Equals(affiliation, StringComparison.OrdinalIgnoreCase))
                                .OrderBy(c => c.Name)
                                .ToListAsync();

            if (!characters.Any())
            {
                _logger.LogWarning("No characters found in LOCAL DB with affiliation: {Affiliation}", affiliation);
                return NotFound($"No characters found with the affiliation '{affiliation}'.");
            }

            // Mapear a DTO antes de devolver
            var characterDtos = characters.Select(c => new Character
            {
                Id = c.Id,
                Name = c.Name,
                Ki = c.Ki,
                Race = c.Race,
                Gender = c.Gender,
                Description = c.Description,
                Affiliation = c.Affiliation,
                Transformations = c.Transformations.Select(t => new Transformation
                {
                    Id = t.Id,
                    Name = t.Name,
                    Ki = t.Ki
                }).ToList()
            }).ToList();

            return Ok(characterDtos);
        }



        [HttpPost("sync")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SyncCharacters()
        {
            _logger.LogInformation("Received request to sync characters.");

            var (success, message) = await _syncService.SynchronizeCharactersAsync();

            if (success)
            {
                _logger.LogInformation("Sync successful: {Message}", message);
                return Ok(message);
            }
            else if (message.Contains("Database already contains data"))
            {
                _logger.LogWarning("Sync failed: {Message}", message);
                return BadRequest(message);
            }
            else
            {
                _logger.LogError("Sync failed: {Message}", message);

                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }
    }
}