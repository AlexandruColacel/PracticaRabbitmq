using AppForSEII2526.API.DTOs.DeviceDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppForSEII2526.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        //used to enable your controller to access to the database
        private readonly ApplicationDbContext _context;
        //used to log any information when your system is running
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(ApplicationDbContext context, ILogger<DevicesController> logger)
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
            if (op2 == 0)
            {
                _logger.LogError($"{DateTime.Now} Exception: op2=0, division by 0");
                return BadRequest("op2 must be different from 0");
            }
            decimal result = decimal.Round(op1 / op2, 2);
            return Ok(result);
        }

        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IList<DeviceParaRentDTO>), (int)HttpStatusCode.OK)]

        public async Task<IActionResult> GetAllDevices(string? Model, double? RentPrice)
        {
            var devices = await _context.Device
                //filtro2
                .Where(d => (Model == null || d.Model.NameModel.Contains(Model)) 
                        && (RentPrice == null || d.PriceForRent==RentPrice))
                //fitlro1
                .Select(m => new DeviceParaRentDTO(m.id,m.Name,m.Model.NameModel,m.Brand, m.Year,m.Color,m.PriceForRent))
                .ToListAsync();
            return Ok(devices);
        }
    }
}