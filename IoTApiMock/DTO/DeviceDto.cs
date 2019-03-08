using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using LiteDB;

namespace IoTApiMock.DTO
{
    public class DeviceDto
    {
        [BsonId]
        public int Id { get; set; }
        [Range(100000000, 999999999, ErrorMessage = "Value must be between 1000000000 to 9999999999")]
        public int SerialNumber { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long WorkCount { get; set; }
        public long Credit { get; set; }
        public DateTime LastService { get; set; }
        public DateTime NextService { get; set; }
        public byte[] PrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
        public bool IsWorking { get; set; }
        public bool IsInterrupted { get; set; }
        public List<string> Tokens { get; set; }
        
    }
}
