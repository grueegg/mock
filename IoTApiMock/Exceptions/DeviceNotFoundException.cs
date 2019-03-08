using System;

namespace IoTApiMock.Exceptions
{
    public class DeviceNotFoundException : Exception
    {
        public DeviceNotFoundException(string message) : base(message)
        {
        }
    }
}
