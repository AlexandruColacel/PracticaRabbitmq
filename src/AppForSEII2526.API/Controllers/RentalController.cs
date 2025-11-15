
using AppForSEII2526.API.DTOs.RentalDTOs;
using AppForSEII2526.API.Data;
using AppForSEII2526.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RentalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RentalController> _logger;

        public RentalController(ApplicationDbContext context, ILogger<RentalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(RentalDetailsDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateRental([FromBody] RentalPostDTO rentalForCreate)
        {
            // Validaciones de fechas
            if (rentalForCreate.RentalDateFrom <= DateTime.Today)
                ModelState.AddModelError("RentalDateFrom", "Error! Your rental date must start later than today");

            if (rentalForCreate.RentalDateFrom >= rentalForCreate.RentalDateTo)
                ModelState.AddModelError("RentalDateFrom&RentalDateTo", "Error! Your rental must end later than it starts");

            if (rentalForCreate.RentalItems == null || rentalForCreate.RentalItems.Count == 0)
                ModelState.AddModelError("RentalItems", "Error! You must include at least one device to be rented");

            // Validar usuario
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == rentalForCreate.CustomerUserName);
            if (user == null)
                ModelState.AddModelError("CustomerUserName", "Error! UserName is not registered");

            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // Obtener IDs de dispositivos desde el DTO
            var deviceIds = rentalForCreate.RentalItems.Select(ri => ri.Id).ToList();

            // Consultar dispositivos con información de alquileres existentes
            var devices = await _context.Device
                .Include(d => d.Model)
                .Include(d => d.DeviceItems)
                    .ThenInclude(rd => rd.Rental)
                .Where(d => deviceIds.Contains(d.id))
                .Select(d => new
                {
                    d.id,
                    d.Name,
                    ModelName = d.Model.NameModel,
                    d.Brand,
                    d.QuantityForRent,
                    d.PriceForRent,
                    // Contar dispositivos ya alquilados en el período
                    NumberOfRentedDevices = d.DeviceItems.Count(rd =>
                        rd.Rental.RentalDateFrom <= rentalForCreate.RentalDateTo &&
                        rd.Rental.RentalDateTo >= rentalForCreate.RentalDateFrom)
                })
                .ToListAsync();

            // Crear el objeto Rental
            var rental = new Rental
            {
                Name = rentalForCreate.CustomerNameSurname.Split(' ')[0], // Primer nombre
                Surname = rentalForCreate.CustomerNameSurname.Contains(' ')
                    ? rentalForCreate.CustomerNameSurname.Substring(rentalForCreate.CustomerNameSurname.IndexOf(' ') + 1)
                    : string.Empty,
                ApplicationUser = user,
                DeliveryAddress = rentalForCreate.DeliveryAddress,
                RentalDate = DateTime.Now,
                PaymentMethod = rentalForCreate.PaymentMethod,
                RentalDateFrom = rentalForCreate.RentalDateFrom,
                RentalDateTo = rentalForCreate.RentalDateTo,
                RentDevices = new List<RentDevice>()
            };

            rental.TotalPrice = 0;
            var numDays = (int)Math.Ceiling((rental.RentalDateTo.Value - rental.RentalDateFrom.Value).TotalDays);

            // Validar disponibilidad y crear RentDevice para cada item
            foreach (var item in rentalForCreate.RentalItems)
            {
                var device = devices.FirstOrDefault(d => d.id == item.Id);

                if (device == null)
                {
                    ModelState.AddModelError("RentalItems", $"Error! Device with ID '{item.Id}' does not exist");
                    continue;
                }

                // Verificar disponibilidad considerando la cantidad solicitada
                var availableQuantity = device.QuantityForRent - device.NumberOfRentedDevices;
                if (availableQuantity < item.Quantity)
                {
                    ModelState.AddModelError("RentalItems",
                        $"Error! Device '{device.Brand} {device.ModelName}' only has {availableQuantity} units available for rent from {rentalForCreate.RentalDateFrom.ToShortDateString()} to {rentalForCreate.RentalDateTo.ToShortDateString()}. You requested {item.Quantity} units.");
                }
                else
                {
                    // Crear RentDevice
                    var rentDevice = new RentDevice
                    {
                        DeviceId = device.id,
                        Rental = rental,
                        RentId = rental.Id, // Se asignará automáticamente al guardar
                        Price = device.PriceForRent,
                        Quantity = item.Quantity
                    };

                    rental.RentDevices.Add(rentDevice);

                    // Actualizar información completa en el DTO para la respuesta
                    item.RentPrice = device.PriceForRent;
                    item.Model = device.ModelName;
                    item.Brand = device.Brand;
                }
            }

            // Calcular precio total
            rental.TotalPrice = rental.RentDevices.Sum(rd => rd.Price * rd.Quantity * numDays);

            // Si hay errores de disponibilidad
            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // Guardar en la base de datos
            _context.Add(rental);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving rental");
                return Conflict("Error: There was an error while saving your rental, please try again later. " + ex.Message);
            }

            // Construir DTO de respuesta según el enunciado
            var rentalDetails = new RentalDetailsDTO(
                rental.Id,
                rental.RentalDate.Value,
                rental.ApplicationUser.UserName,
                rentalForCreate.CustomerNameSurname,
                rental.DeliveryAddress,
                rental.PaymentMethod,
                rental.RentalDateFrom.Value,
                rental.RentalDateTo.Value,
                rentalForCreate.RentalItems,
                rental.TotalPrice.Value
            );

            return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rentalDetails);
        }
        //METODO POST
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RentalDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetRental(int id)
        {
            var rental = await _context.Rental //pillo el rental para mostrar los datos que me dicta el CU
                .Include(r => r.RentDevices)  //incluye los rent device que hayan con el id del metodo  
                    .ThenInclude(rd => rd.Device)//para cada alquiler incluye tambien el dispositivos alquiladp
                        .ThenInclude(d => d.Model)//para cada alquiler incluye tambien el modelo
                .Include(r => r.ApplicationUser) //incluye el usuario(AplicationUsser)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rental == null)
                return NotFound();

            // Construir lista de items para el DTO (modelo, precio, cantidad)
            var items = rental.RentDevices.Select(rd => new RentalItemDTO(
                rd.DeviceId,
                rd.Device.Model.NameModel,
                rd.Device.Brand,
                rd.Price,
                rd.Quantity
            )).ToList();

            var rentalDetails = new RentalDetailsDTO(
                rental.Id,
                rental.RentalDate ?? DateTime.Now,
                rental.ApplicationUser.UserName,
                $"{rental.Name} {rental.Surname}",
                rental.DeliveryAddress,
                rental.PaymentMethod,
                rental.RentalDateFrom ?? DateTime.Now,
                rental.RentalDateTo ?? DateTime.Now,
                items,
                rental.TotalPrice ?? 0
            );

            return Ok(rentalDetails);
        }
    }
}