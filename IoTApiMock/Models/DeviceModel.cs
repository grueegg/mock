using System;

namespace IoTApiMock.Models
{
    public class DeviceModel
    {
        public int SerialNumber { get; set; }
        public string Name { get; set; }
        public long WorkCount { get; set; }
        public long Credit { get; set; }
        public DateTime LastService { get; set; }
        public DateTime NextService { get; set; }
        public bool IsWorking { get; set; }


    }

}
