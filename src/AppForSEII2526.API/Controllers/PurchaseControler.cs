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
                        pi.Device.QuantityForPurchase, // <-- si PurchaseItem tiene la propiedad Quantity, usa esa en lugar de Device
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

        //METODO POST (en producción)

        //[HttpPost]
        //[Route("[action]")]
        //[ProducesResponseType(typeof(PurchaseDetailsDTO), (int)HttpStatusCode.Created)]
        //[ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        //public async Task<ActionResult> CreatePurchase([FromBody] PurchaseForCreateDTO purchaseForCreate)
        //{
        //    if (purchaseForCreate == null)
        //        return BadRequest(new ValidationProblemDetails(ModelState));

        //    // Validaciones de negocio
        //    if (purchaseForCreate.PurchaseItems == null || !purchaseForCreate.PurchaseItems.Any())
        //        ModelState.AddModelError("PurchaseItems", "You must include at least one device to purchase.");

        //    // Comprobar usuario existe
        //    var user = _context.ApplicationUsers.FirstOrDefault(au => au.UserName == purchaseForCreate.CustomerUserName);
        //    if (user == null)
        //        ModelState.AddModelError("PurchaseApplicationUser", "UserName is not registered.");

        //    if (ModelState.ErrorCount > 0)
        //        return BadRequest(new ValidationProblemDetails(ModelState));

        //    // Obtener ids y cargar dispositivos necesarios
        //    var deviceIds = purchaseForCreate.PurchaseItems.Select(pi => pi.DeviceId).Distinct().ToList();

        //    var devices = await _context.Device
        //        .Include(d => d.Model)
        //        .Where(d => deviceIds.Contains(d.id))
        //        .ToListAsync();

        //    // Validar existencia y stock
        //    foreach (var item in purchaseForCreate.PurchaseItems)
        //    {
        //        var device = devices.FirstOrDefault(d => d.id == item.DeviceId);
        //        if (device == null)
        //        {
        //            ModelState.AddModelError("PurchaseItems", $"Device with id {item.DeviceId} does not exist.");
        //            continue;
        //        }

        //        if (device.QuantityForPurchase < item.Quantity)
        //            ModelState.AddModelError("PurchaseItems", $"Not enough stock for device {device.Brand} {device.Name}. Available {device.QuantityForPurchase}, requested {item.Quantity}.");
        //    }

        //    if (ModelState.ErrorCount > 0)
        //        return BadRequest(new ValidationProblemDetails(ModelState));

        //    // Begin transaction (para garantizar atomicidad si actualizas stock)
        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        // Crear entidad Purchase
        //        var purchase = new Purchase
        //        {
        //            CustomerUserName = purchaseForCreate.CustomerUserName,
        //            CustomerUserSurname = purchaseForCreate.CustomerUserSurname,
        //            DeliveryAddress = purchaseForCreate.DeliveryAddress,
        //            PaymentMethod = purchaseForCreate.PaymentMethod,
        //            PurchaseDate = DateTime.UtcNow,
        //            TotalPrice = 0.0,
        //            Quantity = 0,
        //            ApplicationUser = user,
        //            PurchaseItems = new List<PurchaseItem>()
        //        };

        //        double totalPrice = 0.0;
        //        int totalQuantity = 0;

        //        // Mapear DTOs a entidades PurchaseItem
        //        foreach (var itemDto in purchaseForCreate.PurchaseItems)
        //        {
        //            var device = devices.First(d => d.id == itemDto.DeviceId);

        //            // Si tu entidad PurchaseItem tiene un constructor especial para POST, puedes usarlo:
        //            // ejemplo: new PurchaseItem(device.Id, purchase, device.PriceForPurchase, itemDto.Quantity, itemDto.Description)
        //            // si no, rellenamos propiedades manualmente:
        //            var purchaseItem = new PurchaseItem
        //            {
        //                Device = device,
        //                Purchase = purchase,
        //                UnitPrice = device.PriceForPurchase, // precio de la BBDD
        //                Quantity = itemDto.Quantity,
        //                Description = itemDto.Description
        //            };

        //            // añadir PurchaseItem a la compra
        //            purchase.PurchaseItems.Add(purchaseItem);

        //            // actualizar stock (si corresponde)
        //            device.QuantityForPurchase -= itemDto.Quantity;

        //            totalPrice += purchaseItem.UnitPrice * purchaseItem.Quantity;
        //            totalQuantity += purchaseItem.Quantity;
        //        }

        //        purchase.TotalPrice = totalPrice;
        //        purchase.Quantity = totalQuantity;

        //        _context.Purchase.Add(purchase);

        //        // Guardamos cambios (guardará purchase, purchaseItems y el update de devices)
        //        await _context.SaveChangesAsync();

        //        // Commit
        //        await transaction.CommitAsync();

        //        // Construir DTO de respuesta (PurchaseDetailsDTO)
        //        var itemsDto = purchase.PurchaseItems.Select(pi => new PurchaseItemDTO(
        //            pi.Device.Brand,
        //            pi.Device.Model?.NameModel ?? string.Empty,
        //            pi.Device.Color,
        //            (decimal)pi.UnitPrice,   // tu PurchaseItemDTO usa decimal en constructor
        //            pi.Quantity,
        //            pi.Description
        //        )).ToList();

        //        var purchaseDetail = new PurchaseDetailsDTO(
        //            purchase.Id,
        //            purchase.CustomerUserName,
        //            purchase.CustomerUserSurname,
        //            purchase.DeliveryAddress,
        //            purchase.PurchaseDate,
        //            purchase.TotalPrice,
        //            purchase.Quantity,
        //            itemsDto
        //        );

        //        return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, purchaseDetail);
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Error saving purchase");
        //        ModelState.AddModelError("Purchase", $"Error while saving purchase: {ex.Message}");
        //        return Conflict("Error while saving purchase");
        //    }
        //}


    } //De public class PurchaseControler
}//De namespace AppForSEII2526.API.Controllers
