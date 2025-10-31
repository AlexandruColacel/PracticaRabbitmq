using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AppForSEII2526.API.DTOs.DeviceDTO;


namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase //Tiene todo lo que tenga controllerBase (herencia)
    {
        //used to enable your controller to access to the database
        private readonly ApplicationDbContext _context;
        //used to log any information when your system is running
        private readonly ILogger<DeviceController> _logger;

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

        //SELECT: CU 2

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaRentDTO>), (int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetDevicesParaRent(string? Model, double? RentPrice)
        {
            var devices = await _context.Device
                //filtro2
                .Where(d => (Model == null || d.Model.NameModel.Contains(Model))
                        && (RentPrice == null || d.PriceForRent == RentPrice))
                //fitlro1
                .Select(m => new DeviceParaRentDTO(m.id, m.Name, m.Model.NameModel, m.Brand, m.Year, m.Color, m.PriceForRent))
                .ToListAsync();
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
