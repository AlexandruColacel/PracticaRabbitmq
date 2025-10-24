using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using AppForSEII2526.API.DTOs.PurchaseDTO;


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
        public async Task<ActionResult> GetDeviceDTOs()
        {
            var devices = await _context.Device.ToListAsync();

            return Ok(devices);
        }

        //Select: CU 1
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaCompraDTOs>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult> GetDevicesParaRent(string? nombreFILTRO, string? colorFILTRO)
        {
            var devices = await _context.Device
                //2.1 El sistema ofrece al cliente la alternativa de filtrar los dispositivos según su nombre y/o color.
                .Where(d => (nombreFILTRO == null || d.Name.Contains(nombreFILTRO)) &&
                            (colorFILTRO == null || d.Color.Contains(colorFILTRO)))
                .Select(d => new DeviceParaCompraDTOs
                (
                    d.id, //CUIDADO CON LOS TIPOS DE LAS VARIABLES, TANTO EN DTO COMO CLASE ORIGINAL DEBEN 
                    d.Name,
                    d.Brand,
                    d.Model.NameModel,
                    d.Color,
                    d.PriceForPurchase
                ))
                .ToListAsync();
            return Ok(devices);
        }

    }//De public class ValuesController : ControllerBase
}//de namespace AppForSEII2526.API.Controllers
