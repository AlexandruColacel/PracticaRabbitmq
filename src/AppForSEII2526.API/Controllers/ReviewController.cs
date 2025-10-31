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
             .Where(r => r.ReviewId == id)
                 .Include(r => r.ReviewItems) //join table ReviewItems
                    .ThenInclude(ri => ri.Device) //then join table Device
                        .ThenInclude(Device => Device.Model) //then join table Model
             .Select(r => new ReviewDetailDTO(r.ReviewId, r.DateOfReview,r.ReviewTitle, r.CustomerId,r.CustomerId,
                    r.ReviewItems
                        .Select(ri => new ReviewItemDTO(ri.Device.id,
                                ri.Device.Name, ri.Device.Model.NameModel,
                                ri.Device.Year,ri.Rating, ri.Comments)).ToList<ReviewItemDTO>()))
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
