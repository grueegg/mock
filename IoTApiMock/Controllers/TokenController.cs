using IoTApiMock.Models;
using IoTApiMock.Services;
using Microsoft.AspNetCore.Mvc;

namespace IoTApiMock.Controllers
{
    [Route("api/tokens")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private readonly DeviceService _deviceService;
        public TokenController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        /// <summary>
        /// Issue new Token for a specific device
        /// </summary>
        /// <response code="404">In case device was not found</response>
        /// <response code="500">In case of general error</response>
        [ProducesResponseType(200, Type = typeof(string))]
        [HttpPost]
        public IActionResult GetToken([FromBody]TokenCreateModel tokenData)
        {
            var device = _deviceService.GetDevice(tokenData.SerialNumber);
            return Ok(Token.CreateToken(device.SerialNumber.ToString(), tokenData.Count, device.PrivateKey));
        }
    }
}