using System.Collections.Generic;
using AutoMapper;
using IoTApiMock.DTO;
using IoTApiMock.Exceptions;
using IoTApiMock.Models;
using IoTApiMock.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Token = IoTApiMock.Services.Token;

namespace IoTApiMock.Controllers
{

    [Route("api/devices")]
    [ApiController]
    public class DeviceController : ControllerBase
    {

        private readonly DeviceService _deviceService;
        private readonly IMapper _mapper;

        public DeviceController(DeviceService deviceService, IMapper mapper, IDistributedCache cache)
        {
            _mapper = mapper;
            _deviceService = deviceService;
        }

        /// <summary>
        /// Get a List of all available Devices. 
        /// </summary>
        /// <remarks>
        /// If no device is available this method will create a new device.
        /// </remarks>
        /// <response code="500">In case of general error</response>
        [ProducesResponseType(200, Type = typeof(List<DeviceModel>))]
        [HttpGet]
        public IActionResult GetOrCreateDeviceInformation()
        {
            List<DeviceDto> devices;
            try
            {
                devices = _deviceService.GetDevices();

            }
            catch (DeviceNotFoundException)
            {
                devices = new List<DeviceDto> { _deviceService.CreateNewDevice() };
            }
            return Ok(_mapper.Map<List<DeviceDto>, List<DeviceModel>>(devices));
        }

        /// <summary>
        /// Gets a specific device
        /// </summary>
        /// <param name="serialNumber">The serial number of your device</param>
        /// <response code="404">In case device was not found</response>
        /// <response code="500">In case of general error</response>
        [ProducesResponseType(200, Type = typeof(DeviceModel))]
        [HttpGet("{serialNumber}")]
        public IActionResult GetSingleDevice(int serialNumber)
        {

            var devices = _deviceService.GetDevice(serialNumber);
            return Ok(_mapper.Map<DeviceModel>(devices));
        }
        /// <summary>
        /// Returns tokens already being submitted for this device
        /// </summary>
        /// <param name="serialNumber">The serial number of your device</param>
        /// <response code="404">In case device was not found</response>
        /// <response code="500">In case of general error</response>
        [ProducesResponseType(200, Type = typeof(List<TokenModel>))]
        [HttpGet("{serialNumber}/tokens")]
        public IActionResult GetTokens(int serialNumber)
        {
            var device = _deviceService.GetDevice(serialNumber);
            var tokenList = new List<TokenModel>();
            if (device.Tokens == null)
            {
                return Ok(tokenList);
            }
            foreach (var token in device.Tokens)
            {
                tokenList.Add(new TokenModel()
                {
                    Credit = Token.DecodeAndVerifyToken(token, device.PublicKey).Count,
                    DeviceSerialNumber = serialNumber,
                    Key = token
                });
            }
            return Ok(tokenList);
        }

        /// <summary>
        /// Add Token to deivce
        /// </summary>
        /// <param name="serialNumber">The serial number of your device</param>
        /// <response code="204">If the token was successfully added</response>
        /// <response code="400">In case the token is invalid</response>
        /// <response code="404">In case device was not found</response>
        /// <response code="500">In case of general error</response>
        [HttpPost("{serialNumber}/tokens")]
        public IActionResult AddWorkToken(int serialNumber, [FromBody] TokenSubmitModel tokenModel)
        {
            try
            {
                _deviceService.AddToken(serialNumber, tokenModel.Key);
                return NoContent();
            }
            catch (InvalidTokenException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /// <summary>
        /// Turn the specific device on
        /// </summary>
        /// <param name="serialNumber">The serial number of your device</param>
        /// <response code="200">If the deivce has been successfully turned on</response>
        /// <response code="404">In case device was not found</response>
        /// <response code="405">If device does not start because it's already running or the device have no valid credit.</response>
        /// <response code="500">In case of general error</response>
        [HttpPut("{serialNumber}/start")]
        public IActionResult StartWork(int serialNumber)
        {

            return _deviceService.StartWork(serialNumber) ? Ok() : StatusCode(405);

        }
        /// <summary>
        /// Turn the specific device off
        /// </summary>
        /// <param name="serialNumber">The serial number of your device</param>
        /// <response code="200">If device has been successfully turned off, is also returned if the device has already been switched off</response>
        /// <response code="404">In case device was not found</response>
        /// <response code="500">In case of general error</response>
        [HttpPut("{serialNumber}/stop")]
        public IActionResult StopWork(int serialNumber)
        {

            _deviceService.StopWork(serialNumber);
            return Ok();

        }
    }

}