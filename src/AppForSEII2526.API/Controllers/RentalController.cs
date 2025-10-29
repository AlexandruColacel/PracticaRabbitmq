using AppForSEII2526.API.DTOs.RentalDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        //used to enable your controller to access to the database
        private readonly ApplicationDbContext _context;
        //used to log any information when your system is running
        private readonly ILogger<DevicesController> _logger;

        public RentalController(ApplicationDbContext context, ILogger<DevicesController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ActionResult> GetRental(int id)
        {
            if (_context.Rental == null)
            {
                _logger.LogError("Error: Rentals table does not exist");
                return NotFound();
            }

            var rental = await _context.Rental
             .Where(r => r.Id == id)
                 .Include(r => r.RentDevices) //join table RentalDevice
                    .ThenInclude(ri => ri.Device) //then join table Device
                        .ThenInclude(dispositivo => dispositivo.Model) //then join table Genre
             .Select(r => new RentalDetailDTO(r.Id, r.RentalDate, r.CustomerUserName,
                    r.CustomerNameSurname, r.DeliveryAddress,
                    (PaymentMethodTypes)r.PaymentMethod,
                    r.RentalDateFrom, r.RentalDateTo,
                    r.RentalItems
                        .Select(ri => new RentalItemDTO(ri.Movie.Id,
                                ri.Movie.Title, ri.Movie.Genre.Name,
                                ri.Movie.PriceForRenting, ri.Description)).ToList<RentalItemDTO>()))
             .FirstOrDefaultAsync();


            if (rental == null)
            {
                _logger.LogError($"Error: Rental with id {id} does not exist");
                return NotFound();
            }


            return Ok(rental);
        }





    }
}
