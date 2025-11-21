using AppForSEII2526.API.DTOs.DeviceDTO;
using AppForSEII2526.API.DTOs.PurchaseDTO;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Policy;

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
                        // cantidad: la cantidad específica de este item (PurchaseItem.Quantity)
                        pi.Quantity,
                        // descripción del item (explicación del cliente)
                        pi.Description ?? string.Empty
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
            if (purchaseForCreate == null)
            {
                // BadRequest por DTO nulo
                ModelState.AddModelError("Purchase", "The purchase data cannot be null.");
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // --- 1. Validaciones de Negocio (similar a RentalController) ---
            // Validar que hay items en el carrito
            if (purchaseForCreate.PurchaseItems == null || !purchaseForCreate.PurchaseItems.Any())
                ModelState.AddModelError("PurchaseItems", "You must include at least one device to purchase.");

            // Comprobar que el usuario existe (similar a RentalController)
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == purchaseForCreate.CustomerUserName);
            if (user == null)
                ModelState.AddModelError("CustomerUserName", "UserName is not registered.");

            // Si hay errores de validación iniciales, devolver BadRequest
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));


            // --- 2. Obtener y Validar Dispositivos y Stock (lógica adaptada) ---
            var deviceIds = purchaseForCreate.PurchaseItems.Select(pi => pi.Id).Distinct().ToList();
            var devices = await _context.Device
                .Include(d => d.Model)
                .Where(d => deviceIds.Contains(d.id))
                .ToListAsync();

            foreach (var itemDto in purchaseForCreate.PurchaseItems)
            {
                var device = devices.FirstOrDefault(d => d.id == itemDto.Id);

                if (device == null)
                {
                    ModelState.AddModelError("PurchaseItems", $"Device with id {itemDto.Id} does not exist.");
                    continue;
                }

                if (itemDto.Quantity <= 0)
                {
                    ModelState.AddModelError("PurchaseItems", $"Quantity for device {device.Brand} {device.Model?.NameModel} must be greater than 0.");
                }

                // Validación de stock 
                if (device.QuantityForPurchase < itemDto.Quantity)
                {
                    ModelState.AddModelError("PurchaseItems", $"Not enough stock for device '{device.Brand} {device.Model?.NameModel}'. Available: {device.QuantityForPurchase}, requested: {itemDto.Quantity}.");
                }

                //EXAMEN: validación de que brand, no tenga "Xiaomi" o "Huawei"
                if(device.Brand.Contains("Huawei") || device.Brand.Contains("Xiaomi"))
                {
                    //return BadRequest("Error: las tecnologías de la marca Xiaomi y/o Huawei ya no estan disponibles, siguiendo recomendacions de las autoridades competentes en materia de seguridad");
                    ModelState.AddModelError("PurchaseItems", $"Invalid brand '{device.Brand}'.");
                }
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // --- 3. Iniciar Transacción y Crear Entidades ---
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var purchase = new Purchase
                {
                    CustomerUserName = purchaseForCreate.CustomerUserName,
                    CustomerUserSurname = purchaseForCreate.CustomerUserSurname,
                    DeliveryAddress = purchaseForCreate.DeliveryAddress,
                    PaymentMethod = purchaseForCreate.PaymentMethod,
                    PurchaseDate = DateTime.UtcNow,
                    ApplicationUser = user,
                    PurchaseItems = new List<PurchaseItem>()
                };

                double totalPrice = 0.0;
                int totalQuantity = 0;

                foreach (var itemDto in purchaseForCreate.PurchaseItems)
                {
                    var device = devices.First(d => d.id == itemDto.Id);

                    var purchaseItem = new PurchaseItem
                    {
                        Device = device,
                        Purchase = purchase,
                        Price = device.PriceForPurchase,
                        Quantity = itemDto.Quantity,
                        Description = itemDto.Description
                    };

                    purchase.PurchaseItems.Add(purchaseItem);

                    device.QuantityForPurchase -= itemDto.Quantity;

                    totalPrice += purchaseItem.Price * purchaseItem.Quantity;
                    totalQuantity += purchaseItem.Quantity;
                }

                purchase.TotalPrice = totalPrice;
                purchase.Quantity = totalQuantity;

                _context.Purchase.Add(purchase);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var itemsDto = purchase.PurchaseItems.Select(pi => new PurchaseItemDTO(
                    pi.Device.id,
                    pi.Device.Brand,
                    pi.Device.Model?.NameModel ?? string.Empty,
                    pi.Device.Color,
                    (decimal)pi.Device.PriceForPurchase,
                    pi.Quantity,
                    pi.Description
                )).ToList();

                var purchaseDetail = new PurchaseDetailsDTO(
                    purchase.Id,
                    purchase.CustomerUserName,
                    purchase.CustomerUserSurname,
                    purchase.DeliveryAddress,
                    purchase.PurchaseDate,
                    (decimal)purchase.TotalPrice,
                    purchase.Quantity,
                    itemsDto
                );

                return CreatedAtAction(nameof(GetPurchase), new { id = purchase.Id }, purchaseDetail);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving purchase");
                ModelState.AddModelError("Purchase", $"Error while saving purchase: {ex.Message}");
                return Conflict("Error while saving purchase");
            }
        }//De createPurchase
    } //De public class PurchaseControler


} //de namespace appforseii2526.api.controllers

