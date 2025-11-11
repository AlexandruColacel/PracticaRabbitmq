using AppForSEII2526.API.DTOs.DeviceDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppForSEII2526.API.Controllers
{
    //metodo get

    [Route("api/[controller]")]
    [ApiController]
    public class DevicesReseñasController : ControllerBase
    {
        //used to enable your controller to access to the database
        private readonly ApplicationDbContext _context;
        //used to log any information when your system is running
        private readonly ILogger<DevicesReseñasController> _logger;

        public DevicesReseñasController(ApplicationDbContext context, ILogger<DevicesReseñasController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(decimal), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> ComputeDivision(decimal op1, decimal op2)
        {
            if (op2 == 0) {
                _logger.LogError($"{DateTime.Now} Exception: op2=0, division by 0");
                return BadRequest("op2 must be different from 0");
            }
            decimal result = decimal.Round(op1 / op2, 2);
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaReseñasDTO>), (int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetAllDevices(string? marca, int? año)
        {
            var devices = await _context.Device
                .Where(m => (m.Brand.Contains(marca)) || (marca == null))
                .Where(m => (m.Year == año) || (año == null))
                .Select(m => new DeviceParaReseñasDTO(m.id, m.Name, m.Brand, m.Color, m.Year, m.Model.NameModel))
                .ToListAsync();
            return Ok(devices);
        }


        //la lista de dispositivos, indicando el nombre, la marca, el color, el año, y el modelo.


    }
}
