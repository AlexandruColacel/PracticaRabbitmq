using Microsoft.EntityFrameworkCore;
using AppForSEII2526.API.DTOs.ReviewDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
     
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ApplicationDbContext context, ILogger<ReviewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //HACER DOS MÉTODOS, GETreview Y CREATEreview

        

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReviewDetailDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetReview(int id)
        {
            if (_context.Review == null)
            {
                _logger.LogError("Error: Review table does not exist");
                return NotFound();
            }

            var review = await _context.Review
             .Where(r => r.Id == id)
                 .Include(r => r.RentalItems) //join table ReviewItems
                    .ThenInclude(ri => ri.Device) //then join table Device
                        .ThenInclude(Device => Device.Model) //then join table Model
             .Select(r => new ReviewDetailDTO(r.Id, r.ReviewDate,r.ReviewTitle, r.NombreCliente,r.PaisCliente,
                    r.ReviewItems
                        .Select(ri => new ReviewItemDTO(ri.Device.id,
                                ri.Device.name, ri.Device.Model.nameModel,
                                ri.Device.Year, ri.Comments)).ToList<ReviewItemDTO>()))
             .FirstOrDefaultAsync();
            
            if (review == null)
            {
                _logger.LogError($"Error: Review with id {id} does not exist");
                return NotFound();
            }


            return Ok(review);
        }
    }
}
