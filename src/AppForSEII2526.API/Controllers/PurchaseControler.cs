using AppForSEII2526.API.DTOs.DeviceDTO;
using AppForSEII2526.API.DTOs.PurchaseDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseControler : ControllerBase //Herencia de controllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseControler> _logger;

        public PurchaseControler(ApplicationDbContext context, ILogger<PurchaseControler> logger)
        {
            _context = context;
            _logger = logger;
        }

        //METODO DETAILS

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(PurchaseDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetPurchase(int id)
        {
            if (_context.Purchase == null)
            {
                _logger.LogError("Error: Purchase table does not exist");
                return NotFound();
            }

            var purchase = await _context.Purchase
                .Where(r => r.Id == id)
                .Include(r => r.PurchaseItems)
                    .ThenInclude(pi => pi.Device)
                        .ThenInclude(d => d.Model)
                .Select(r => new PurchaseDetailsDTO(
                    r.Id,
                    r.CustomerUserName,
                    r.CustomerUserSurname,
                    r.DeliveryAddress,
                    r.PurchaseDate,
                    // conviene convertir a decimal; si tu entidad usa double, puedes castear:
                    (decimal)r.TotalPrice,
                    r.Quantity,
                    r.PurchaseItems.Select(pi => new PurchaseItemDTO(
                        pi.Device.id,
                        pi.Device.Brand,
                        // NO pasamos la entidad Model, pasamos su nombre (campo escalar)
                        pi.Device.Model.NameModel,
                        pi.Device.Color,
                        // precio desde Device
                        (decimal)pi.Device.PriceForPurchase,
                        // cantidad: depende de dónde guardes la cantidad por item:
                        // normalmente Quantity viene de PurchaseItem (si existe) o de Device.QuantityForPurchase
                        // aquí supongo que PurchaseItem contiene la cantidad (si no, usar Device.QuantityForPurchase)
                        d.Model.Quantity, // <-- si PurchaseItem tiene la propiedad Quantity, usa esa en lugar de Device
                        pi.Device.Description ?? string.Empty
                    )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (purchase == null)
            {
                _logger.LogError($"Error: Purchase with id {id} does not exist");
                return NotFound();
            }

            return Ok(purchase);
        }

        //METODO POST - CREATE PURCHASE
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(PurchaseDetailsDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreatePurchase([FromBody] PurchaseForCreateDTO purchaseForCreate)
        {

        }


    } //De public class PurchaseControler
}//De namespace AppForSEII2526.API.Controllers
