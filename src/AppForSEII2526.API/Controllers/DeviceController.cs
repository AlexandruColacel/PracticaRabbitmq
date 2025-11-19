using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AppForSEII2526.API.DTOs.DeviceDTO;


namespace AppForSEII2526.API.Controllers
{
    // Defino que esta clase es un Controlador de API.
    // La ruta será "api/Device", porque la clase se llama DeviceController.
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase //Tiene todo lo que tenga controllerBase (herencia)
    {
        // 1. HERRAMIENTAS QUE NECESITO
        // Declaro las variables privadas donde guardaré mis herramientas de trabajo
        // _context: Es mi "puente" a la Base de Datos. Sin esto no puedo leer ni guardar nada.
        private readonly ApplicationDbContext _context;
        
        // _logger: Es mi "diario de a bordo". Aquí apunto si algo sale mal para poder revisarlo luego.
        private readonly ILogger<DeviceController> _logger;

        // 2. CONSTRUCTOR (INYECCIÓN DE DEPENDENCIAS)
        // El sistema me da (inyecta) automáticamente el contexto y el logger.
        // Yo solo tengo que guardarlos en mis variables privadas para usarlos después.
        public DeviceController(ApplicationDbContext context, ILogger<DeviceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Vamos a hacer un método de acción GetComputingProcess (un proceso para dividir entre 2 numeros)
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult> ComputeDivision(decimal op1, decimal op2)
        {
            if (op2 == 0)
            {
                _logger.LogError($"{DateTime.Now}Exception: op2=0, division by 0");
                return BadRequest("op2 must be different from 0");
            }
            decimal result = decimal.Round(op1 / op2, 2);
            return Ok(result);
        }

        //Metodo GET para obtener todos los devices de la base de datos
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaCompraDTOs>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetDevice()
        {
            var devices = await _context.Device.ToListAsync();

            return Ok(devices);
        }

        //Select: CU 1

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaCompraDTOs>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetDevicesParaPurchase(string? nombreFILTRO, string? colorFILTRO)
        {
            var devices = await _context.Device
                //2.1 El sistema ofrece al cliente la alternativa de filtrar los dispositivos según su nombre y/o color.
                .Where(d => (nombreFILTRO == null || d.Name.Contains(nombreFILTRO)) &&
                            (colorFILTRO == null || d.Color.Contains(colorFILTRO)))
                .Select(d => new DeviceParaCompraDTOs
                (
                    d.id, //CUIDADO CON LOS TIPOS DE LAS VARIABLES, revisar clases si es necesario
                    d.Name,
                    d.Brand,
                    d.Model.NameModel,
                    d.Color,
                    d.PriceForPurchase
                ))
                .ToListAsync();
            return Ok(devices);
        }

        // =========================================================================
        // CU 2: OBTENER DISPOSITIVOS PARA ALQUILAR (CATÁLOGO DE ALQUILER)
        // =========================================================================
        // Este método es el que llama la web cuando el usuario quiere ver qué móviles puede alquilar.
        // Permite filtrar por Modelo (ej: "Iphone") y por Precio exacto.

        [HttpGet] // Es una petición de lectura (GET), no voy a modificar nada en la BD.
        [Route("[action]")]// La URL terminará en /GetDevicesParaRent
        [ProducesResponseType(typeof(IList<DeviceParaRentDTO>), (int)HttpStatusCode.OK)]

        // Documento qué tipo de respuesta voy a devolver para que Swagger lo sepa:
        // Si todo va bien (200 OK), devuelvo una LISTA de mi DTO específico (DeviceParaRentDTO).
        public async Task<IActionResult> GetDevicesParaRent(string? Model, double? RentPrice)
        {
            // --- VALIDACIÓN DE ENTRADA (BAD REQUEST) ---
            // Antes de buscar nada, compruebo si el cliente me está pidiendo cosas imposibles.
            if (RentPrice.HasValue && RentPrice < 0) {
                // Añado el error al estado del modelo (diccionario de errores).
                ModelState.AddModelError("RentPrice", "Error! RentPrice cannot be negative");

                // Registro el problema en mi diario (Log).
                _logger.LogError($"{DateTime.Now} Error: RentPrice ({RentPrice}) cannot be negative");

                // Devuelvo 400 Bad Request con los detalles del problema.
                return BadRequest(new ValidationProblemDetails(ModelState));
            }
            // 3. CONSULTA A LA BASE DE DATOS CON LINQ
            // Empiezo a construir mi consulta sobre la tabla 'Device'.
            // Uso 'await' para no congelar el servidor mientras la BD busca los datos.
            var devices = await _context.Device
                // PASO 3.1: FILTRADO (WHERE)
                // Aplico los filtros SOLO si el usuario me ha enviado algo (si no son null).
                // La lógica es: "(El filtro es nulo O el dato coincide)".
                // Filtro por Modelo: Busco en la tabla relacionada 'Model' si el nombre contiene el texto buscado.
                //filtro2
                .Where(d => (Model == null || d.Model.NameModel.Contains(Model))
                        // Filtro por Precio: Busco si el precio de alquiler coincide exactamente.
                        && (RentPrice == null || d.PriceForRent == RentPrice))
                //fitlro1
                // PASO 3.2: PROYECCIÓN (SELECT) -> EL PASE MÁGICO A DTO
                // Aquí es donde transformo los datos "crudos" de la BD (Entidad Device)
                // en mi objeto "bonito" para el cliente (DeviceParaRentDTO).
                // Solo cojo los campos que me interesan para el catálogo de alquiler.
                .Select(m => new DeviceParaRentDTO(m.id, m.Name, m.Model.NameModel, m.Brand, m.Year, m.Color, m.PriceForRent))

                // PASO 3.3: EJECUCIÓN
                // Hasta aquí solo había preparado la "pregunta". 
                // Con .ToListAsync() envío la pregunta a la BD y traigo los resultados a la memoria.

                .ToListAsync();
            // 4. DEVOLVER RESPUESTA
            // Devuelvo un código 200 OK con la lista de dispositivos encontrada (o vacía si no hay coincidencias).
            return Ok(devices);
        }

        //SELECT : CU 3

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaReseñasDTO>), (int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetDevicesParaReview(string? marca, int? año)
        {
            var devices = await _context.Device
                .Where(m => (m.Brand.Contains(marca)) || (marca == null))
                .Where(m => (m.Year == año) || (año == null))
                .Select(m => new DeviceParaReseñasDTO(m.id, m.Name, m.Brand, m.Color, m.Year, m.Model.NameModel))
                .ToListAsync();
            return Ok(devices);
        }

    }//De public class ValuesController : ControllerBase
}//de namespace AppForSEII2526.API.Controllers
