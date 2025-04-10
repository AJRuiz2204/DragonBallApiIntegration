using DragonBallApi.Data;
using DragonBallApi.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DragonBallApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Secure this controller
    public class TransformationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
         private readonly ILogger<TransformationsController> _logger;

        public TransformationsController(ApplicationDbContext context, ILogger<TransformationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/transformations
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Transformation>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Transformation>>> GetTransformations()
        {
             _logger.LogInformation("Getting all transformations from DB");
             
             return await _context.Transformations
                                 .OrderBy(t => t.Name)
                                 .ToListAsync();
        }

         // GET: api/transformations/{id} - Might be less useful if IDs are external, but good practice
         [HttpGet("{id}")]
         [ProducesResponseType(typeof(Transformation), StatusCodes.Status200OK)]
         [ProducesResponseType(StatusCodes.Status404NotFound)]
         public async Task<ActionResult<Transformation>> GetTransformation(int id)
         {
            _logger.LogInformation("Getting transformation with ID: {TransformationId}", id);
             var transformation = await _context.Transformations
                                                .FirstOrDefaultAsync(t => t.Id == id);

             if (transformation == null)
             {
                _logger.LogWarning("Transformation with ID: {TransformationId} not found.", id);
                 return NotFound();
             }

             return transformation;
         }
    }
}