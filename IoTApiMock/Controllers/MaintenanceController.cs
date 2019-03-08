using IoTApiMock.Models;
using IoTApiMock.Services;
using LiteDB;
using Microsoft.AspNetCore.Mvc;

namespace IoTApiMock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private DeviceService _deviceService; 

        public MaintenanceController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }
        /// <summary>
        /// Delete all user produced data
        /// </summary>
        /// <response code="200">If all data has been successfully reseted</response>
        /// <response code="500">In case of general error</response>
        [HttpDelete]
        public IActionResult ResetDatabase()
        {
            using (var db = new LiteDatabase("device.db"))
            {
                db.DropCollection("devices");
            }

            return Ok();
        }

        /// <summary>
        /// Add one more device
        /// </summary>
        /// <response code="200">If Devices has been successfully added</response>
        /// <response code="500">In case of general error</response>
        [HttpPost("Device")]
        public IActionResult AddDevice([FromBody] DeviceCreationModel device)
        {
            _deviceService.CreateNewDevice(device.Name);
            return Ok();
        }
    }
}