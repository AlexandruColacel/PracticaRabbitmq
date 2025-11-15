using AppForSEII2526.API.DTOs.ReviewDTOs;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AppForSEII2526.API.Controllers
{



    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;
        private readonly IReadOnlyList<int> ValoresPermitidos = new List<int> { 1, 5, 10, 20 };//spain, france, germany, italy

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
             .Select(r => new ReviewDetailDTO(r.ReviewId, r.DateOfReview, r.ReviewTitle, r.CustomerId, r.CustomerCountry,
                    r.ReviewItems
                        .Select(ri => new ReviewItemDTO(ri.Device.id,
                                ri.Device.Name, ri.Device.Model.NameModel,
                                ri.Device.Year, ri.Rating, ri.Comments)).ToList<ReviewItemDTO>()))
             .FirstOrDefaultAsync();

            if (review == null)
            {
                _logger.LogError($"Error: Review with id {id} does not exist");
                return NotFound();
            }


            return Ok(review);
        }


        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(ReviewDetailDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]

        public async Task<ActionResult> CreateReview(ReviewForCreateDTO reviewForCreate)
        {

            // any validation defined in ReviewForCreate is checked before running the method so they don't have to be checked again
            if (reviewForCreate.ReviewItems.Count == 0)
                ModelState.AddModelError("ReviewItems", "Error! You must include at least one device to be reviewed");
            var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == reviewForCreate.CustomerId);
            if (user == null)
                ModelState.AddModelError("RentalApplicationUser", "Error! UserName is not registered");
            // Validar pais
            if (!ValoresPermitidos.Contains(reviewForCreate.CustomerCountry))
                ModelState.AddModelError("CustomerCountry", "Error! The country is not valid. Allowed values are: 1 (Spain), 5 (France), 10 (Germany), 20 (Italy)");
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));



            var deviceIds = reviewForCreate.ReviewItems.Select(ri => ri.DeviceId).ToList<int>();

            var devices = _context.Device.Include(m => m.ReviewItems)
                .ThenInclude(ri => ri.Review)
                .Where(m => deviceIds.Contains(m.id))

                .Select(m => new {
                    m.id,
                    m.Model,
                    m.Name,
                    m.Year

                }).ToList();

            Review review = new Review {
                DateOfReview = reviewForCreate.DateOfReview, 
                ReviewTitle = reviewForCreate.ReviewTitle,
                CustomerId = reviewForCreate.CustomerId, 
                CustomerCountry = reviewForCreate.CustomerCountry,
               ReviewItems = new List<ReviewItem>(),
               ApplicationUser = user
            };


            review.OverallRating = 0;
            foreach (var item in reviewForCreate.ReviewItems)
            {
                
                var device = devices.FirstOrDefault(m => m.id == item.DeviceId);
                // we must check that the device exists in the database
                if (device == null)
                {
                    ModelState.AddModelError("ReviewItems", $"Error! Device model '{item.ModelName}' does not exist in the database");
                }
                else
                {
                    // review does not exist in the database yet and does not have a valid Id, so we must relate reviewitem to the object review
                    review.ReviewItems.Add(new ReviewItem(item.DeviceId, item.Comments, item.Rating, review));
                    
                }
            }

            review.OverallRating = review.ReviewItems.Sum(ri => ri.Rating * review.ReviewItems.Count);


            if (ModelState.ErrorCount > 0) {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }


            _context.Add(review);

                try
                {                     // we store in the database both review and its reviewitems
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    ModelState.AddModelError("Review", $"Error! There was an error while saving your review, please, try again later");
                    return Conflict("Error" + ex.Message);
                }



                var reviewDetail = new ReviewDetailDTO(review.ReviewId, review.DateOfReview,
                    review.ReviewTitle, review.CustomerId, review.CustomerCountry,
                    reviewForCreate.ReviewItems);
                return CreatedAtAction("GetReview", new { id = review.ReviewId }, reviewDetail);

                



            
        }
    }
}