
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
        // 1. NECESITO ACCESO A LOS DATOS Y AL LOG
        // Declaro estas variables privadas para guardar las herramientas que necesito:
        // _context: Es mi conexión con la Base de Datos (Entity Framework).
        // _logger: Es mi libreta para apuntar errores o sucesos importantes.
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RentalController> _logger;

        // CONSTRUCTOR: Aquí es donde el sistema me "inyecta" (Dependency Injection) las herramientas.
        // Yo no creo el contexto con 'new', me lo dan listo para usar.
        public RentalController(ApplicationDbContext context, ILogger<RentalController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // -------------------------------------------------------------------------
        // MÉTODO POST: CREAR UN NUEVO ALQUILER
        // -------------------------------------------------------------------------
        // Este método recibe un DTO (la caja con datos) desde el cliente y trata de crear un alquiler.
        // Devuelve: 
        // - 201 Created (con los detalles) si todo va bien.
        // - 400 Bad Request si hay errores de validación.
        // - 409 Conflict si falla la base de datos.
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(typeof(RentalDetailsDTO), (int)HttpStatusCode.Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Conflict)]
        public async Task<ActionResult> CreateRental([FromBody] RentalPostDTO rentalForCreate)
        {
            // Validaciones de fechas
            // PASO 1: VALIDACIONES DE LÓGICA DE NEGOCIO BÁSICAS
            // Antes de molestar a la base de datos, compruebo si los datos tienen sentido lógico.

            // Compruebo: ¿La fecha de inicio es hoy o en el pasado? Eso no vale.
            if (rentalForCreate.RentalDateFrom <= DateTime.Today)
                ModelState.AddModelError("RentalDateFrom", "Error! Your rental date must start later than today");

            // Compruebo: ¿La fecha de fin es anterior a la de inicio? Imposible viajar en el tiempo.
            if (rentalForCreate.RentalDateFrom >= rentalForCreate.RentalDateTo)
                ModelState.AddModelError("RentalDateFrom&RentalDateTo", "Error! Your rental must end later than it starts");

            // Compruebo: ¿La lista de items está vacía? No puedo alquilar "nada".
            if (rentalForCreate.RentalItems == null || rentalForCreate.RentalItems.Count == 0)
                ModelState.AddModelError("RentalItems", "Error! You must include at least one device to be rented");

            // Validar usuario
            // PASO 2: VALIDAR SI EL USUARIO EXISTE
            // Busco en la base de datos si el UserName que me han pasado es real.
            // Uso 'await' porque ir a la BD tarda un poco y no quiero bloquear el servidor.
            var user = await _context.ApplicationUsers.FirstOrDefaultAsync(au => au.UserName == rentalForCreate.CustomerUserName);
            if (user == null)
                ModelState.AddModelError("CustomerUserName", "Error! UserName is not registered");

            // Si he encontrado algún error hasta ahora (fechas mal, usuario fake...), paro aquí y devuelvo error 400.
            if (ModelState.ErrorCount > 0)
                return BadRequest(new ValidationProblemDetails(ModelState));

            // PASO 3: PREPARAR DATOS PARA VALIDAR STOCK (DISPONIBILIDAD)
            // Saco solo los IDs de los dispositivos que el cliente quiere alquilar.
            // Obtener IDs de dispositivos desde el DTO
            var deviceIds = rentalForCreate.RentalItems.Select(ri => ri.Id).ToList();

            // ¡CONSULTA IMPORTANTE! Recupero de la BD la información de esos dispositivos.
            // No me vale solo el dispositivo, necesito saber cuántos están alquilados ya en esas fechas.
            // Consultar dispositivos con información de alquileres existentes
            var devices = await _context.Device
                .Include(d => d.Model)// Me traigo el Modelo para saber su nombre.
                .Include(d => d.DeviceItems)// Me traigo los alquileres previos de este dispositivo.
                    .ThenInclude(rd => rd.Rental)// Y me traigo las fechas de esos alquileres.
                .Where(d => deviceIds.Contains(d.id))// Solo me interesan los dispositivos que ha pedido el cliente.
                .Select(d => new
                {
                    // Selecciono solo lo que necesito para calcular disponibilidad:
                    d.id,
                    d.Name,
                    ModelName = d.Model.NameModel,
                    d.Brand,
                    d.QuantityForRent,// Cuántos tengo en total en el almacén para alquilar.
                    d.PriceForRent,
                    // AQUÍ LA MAGIA: Cuento cuántos de estos dispositivos están ocupados
                    // en el mismo rango de fechas que pide el cliente.
                    // Si un alquiler empieza antes de que termine el mío Y termina después de que empiece el mío, se solapan.
                    // Contar dispositivos ya alquilados en el período
                    NumberOfRentedDevices = d.DeviceItems.Count(rd =>
                        rd.Rental.RentalDateFrom <= rentalForCreate.RentalDateTo &&
                        rd.Rental.RentalDateTo >= rentalForCreate.RentalDateFrom)
                })
                .ToListAsync();

            // Crear el objeto Rental
            // PASO 4: CREAR EL OBJETO RENTAL (LA CABECERA DEL PEDIDO)
            // Empiezo a rellenar la ficha del alquiler con los datos que tengo.
            var rental = new Rental
            {
                // Truco para separar Nombre y Apellido si vienen juntos en el string
                Name = rentalForCreate.CustomerNameSurname.Split(' ')[0], // Primer nombre
                Surname = rentalForCreate.CustomerNameSurname.Contains(' ')
                    ? rentalForCreate.CustomerNameSurname.Substring(rentalForCreate.CustomerNameSurname.IndexOf(' ') + 1)
                    : string.Empty,
                ApplicationUser = user,// Asigno el usuario real que recuperé antes de la BD.
                DeliveryAddress = rentalForCreate.DeliveryAddress,
                RentalDate = DateTime.Now,// Fecha de "ahora" en la que se hace la reserva.
                PaymentMethod = rentalForCreate.PaymentMethod,
                RentalDateFrom = rentalForCreate.RentalDateFrom,
                RentalDateTo = rentalForCreate.RentalDateTo,
                RentDevices = new List<RentDevice>()// Preparo la lista vacía para meter los dispositivos luego.
            };

            // Calculo cuántos días dura el alquiler para cobrarle bien.
            // Math.Ceiling redondea hacia arriba (si es 1.5 días, cobro 2).
            rental.TotalPrice = 0;
            var numDays = (int)Math.Ceiling((rental.RentalDateTo.Value - rental.RentalDateFrom.Value).TotalDays);

            // PASO 5: PROCESAR CADA ÍTEM Y VALIDAR DISPONIBILIDAD REAL
            // Recorro la lista de cosas que pidió el cliente para ver si puedo dárselas.
            // Validar disponibilidad y crear RentDevice para cada item
            foreach (var item in rentalForCreate.RentalItems)
            {
                // Busco este ítem en la lista de dispositivos que me traje de la BD en el Paso 3.  
                var device = devices.FirstOrDefault(d => d.id == item.Id);

                // Si no existe en la BD, error.
                if (device == null)
                {
                    ModelState.AddModelError("RentalItems", $"Error! Device with ID '{item.Id}' does not exist");
                    continue;
                }

                // CÁLCULO DE STOCK: Total en almacén - Los que ya están alquilados esas fechas.
                // Verificar disponibilidad considerando la cantidad solicitada
                var availableQuantity = device.QuantityForRent - device.NumberOfRentedDevices;

                // Si el cliente pide más de los que me quedan libres... ERROR.
                if (availableQuantity < item.Quantity)
                {
                    ModelState.AddModelError("RentalItems",
                        $"Error! Device '{device.Brand} {device.ModelName}' only has {availableQuantity} units available for rent from {rentalForCreate.RentalDateFrom.ToShortDateString()} to {rentalForCreate.RentalDateTo.ToShortDateString()}. You requested {item.Quantity} units.");
                }
                else
                {

                    // ¡HAY STOCK! Creo la línea de detalle (RentDevice).
                    // Esto conecta el Alquiler (Rental) con el Dispositivo (Device).
                    // Crear RentDevice
                    var rentDevice = new RentDevice
                    {
                        DeviceId = device.id,
                        Rental = rental, // Enlazo con el padre.
                        RentId = rental.Id, // Se asignará automáticamente al guardar // (EF asignará esto automáticamente al guardar, pero lo dejo indicado).
                        Price = device.PriceForRent, // Fijo el precio AHORA (por si cambia en el futuro).
                        Quantity = item.Quantity
                    };

                    // Añado esta línea a la lista del alquiler padre.
                    rental.RentDevices.Add(rentDevice);

                    // Actualizo el DTO de respuesta con datos bonitos (Marca, Modelo) para que el cliente los vea.
                    item.RentPrice = device.PriceForRent;
                    item.Model = device.ModelName;
                    item.Brand = device.Brand;
                }
            }

            // Calculo el precio total sumando: PrecioUnitario * Cantidad * Días.
            rental.TotalPrice = rental.RentDevices.Sum(rd => rd.Price * rd.Quantity * numDays);


            // PASO 6: CHECK FINAL DE ERRORES
            // Si durante el bucle encontré algún problema de stock (ModelState tiene errores),
            // devuelvo BadRequest y NO guardo nada en la BD.
            // Si hay errores de disponibilidad
            if (ModelState.ErrorCount > 0)
            {
                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            // PASO 7: GUARDAR EN BASE DE DATOS
            // Añado el alquiler completo (con sus líneas de detalle) al contexto.
            // Guardar en la base de datos
            _context.Add(rental);

            try
            {
                // Intento guardar los cambios físicamente en la BD.
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si falla (ej: se cae la BD justo ahora), lo registro en el log y aviso al cliente.
                _logger.LogError(ex, "Error while saving rental");
                return Conflict("Error: There was an error while saving your rental, please try again later. " + ex.Message);
            }

            // PASO 8: DEVOLVER RESPUESTA 201 CREATED
            // Construyo el DTO de salida (RentalDetailsDTO) con toda la info confirmada.
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
                rentalForCreate.RentalItems,// Devuelvo los items tal como quedaron (con precios y nombres).
                rental.TotalPrice.Value
            );

            // Devuelvo 201 Created.
            // CreatedAtAction añade una cabecera 'Location' que dice dónde consultar este alquiler (GetRental).
            return CreatedAtAction(nameof(GetRental), new { id = rental.Id }, rentalDetails);
        }
        // -------------------------------------------------------------------------
        // MÉTODO GET: CONSULTAR UN ALQUILER POR ID
        // -------------------------------------------------------------------------
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RentalDetailsDTO), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> GetRental(int id)
        {
            // PASO 1: RECUPERAR DATOS COMPLETOS
            // Busco el alquiler por ID, pero uso .Include() masivamente.
            // ¿Por qué? Porque en la BD relacional los datos están separados.
            // Si no hago Include, las propiedades 'RentDevices', 'ApplicationUser', etc., vendrían NULL.
            var rental = await _context.Rental //pillo el rental para mostrar los datos que me dicta el CU  ¡Tráeme las líneas de detalle!
                .Include(r => r.RentDevices)  //incluye los rent device que hayan con el id del metodo   ¡Y para cada línea, tráeme la ficha del dispositivo!
                    .ThenInclude(rd => rd.Device)//para cada alquiler incluye tambien el dispositivos alquiladp ¡Y el modelo de ese dispositivo!
                        .ThenInclude(d => d.Model)//para cada alquiler incluye tambien el modelo ¡Y tráeme los datos del usuario que lo alquiló!
                .Include(r => r.ApplicationUser) //incluye el usuario(AplicationUsser)
                .FirstOrDefaultAsync(r => r.Id == id);

            // Si no encuentro nada, devuelvo 404 Not Found.
            if (rental == null)
                return NotFound();

            // PASO 2: MAPEAR A DTO
            // Convierto las entidades de BD a mi DTO de items para la respuesta.
            // Aquí transformo la estructura compleja de EF en una lista simple para el JSON.
            // Construir lista de items para el DTO (modelo, precio, cantidad)
            var items = rental.RentDevices.Select(rd => new RentalItemDTO(
                rd.DeviceId,
                rd.Device.Model.NameModel,
                rd.Device.Brand,
                rd.Price,
                rd.Quantity
            )).ToList();

            // Construyo el DTO principal de respuesta.
            // Uso el operador ?? (null-coalescing) por seguridad, por si algún campo viniera nulo de BD.
            var rentalDetails = new RentalDetailsDTO(
                rental.Id,
                rental.RentalDate ?? DateTime.Now,
                rental.ApplicationUser.UserName,

                // ¿POR QUÉ ESTO? -> INTERPOLACIÓN DE CADENAS ($)
                // En la Base de Datos (Entity) guardo Nombre y Apellido en columnas separadas por si quiero ordenar.
                // Pero mi DTO (la web) espera un solo campo llamado 'CustomerNameSurname'.
                // Así que aquí los concateno (uno) con un espacio en medio para cumplir con el contrato del DTO.
                $"{rental.Name} {rental.Surname}",
                rental.DeliveryAddress,
                rental.PaymentMethod,
                rental.RentalDateFrom ?? DateTime.Now,
                rental.RentalDateTo ?? DateTime.Now,
                items,
                rental.TotalPrice ?? 0
            );
            // Devuelvo 200 OK con los datos.
            return Ok(rentalDetails);
        }
    }
}