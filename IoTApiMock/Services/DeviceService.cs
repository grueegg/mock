using IoTApiMock.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using IoTApiMock.DTO;
using IoTApiMock.Exceptions;
using LiteDatabase = LiteDB.LiteDatabase;

namespace IoTApiMock.Services
{
    public class DeviceService
    {
        private DeviceModel _deviceModel;
        private readonly IMapper _mapper;
        private readonly string _databaseName = "device.db";
        private readonly string _deviceTable = "devices";
        public DeviceService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public DeviceDto CreateNewDevice(string deviceName = "Inventx-DeviceModel")
        {
            var radom = new Random();
            var device = new DeviceModel()
            {
                Name = deviceName,
                SerialNumber = radom.Next(100000000, 999999999),
                LastService = DateTime.Now,
                NextService = DateTime.Now.AddDays(100),
                IsWorking = false
            };
            var deviceDto = _mapper.Map<DeviceDto>(device);
            var keyPair = Token.CreateKeyPairs();
            deviceDto.PrivateKey = keyPair.privateKey;
            deviceDto.PublicKey = keyPair.publicKey;

            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                collection.Insert(deviceDto);
            }
            return deviceDto;
        }

        public List<DeviceDto> GetDevices()
        {
            List<DeviceDto> devicesDto;
            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                devicesDto = collection.FindAll().ToList();
            }

            if (devicesDto.Count == 0)
            {
                throw new DeviceNotFoundException("No devices found");
            }
            return devicesDto;
        }

        public DeviceDto GetDevice(int serialNumber)
        {
            DeviceDto device;
            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                device = collection.FindOne(d => d.SerialNumber == serialNumber);
            }

            if (device == null)
            {
                throw new DeviceNotFoundException($"Device with serialNumber {serialNumber} was not found");
            }

            return device;
        }


        public void AddToken(int serialNumber, string tokenKey)
        {
            var tokenValue = Token.DecodeAndVerifyToken(tokenKey, GetDevice(serialNumber).PublicKey);
            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                var device = collection.FindOne(d => d.SerialNumber == serialNumber);
                if (device.Tokens != null && device.Tokens.Count > 0 && device.Tokens.Contains(tokenKey))
                {
                    throw new InvalidTokenException("token can not be used twice");
                }
                device.Credit = device.Credit + tokenValue.Count;
                if (device.Tokens == null)
                {
                    device.Tokens = new List<string>();
                }
                device.Tokens.Add(tokenKey);
                collection.Update(device);
            }

        }

        public bool StartWork(int serialNumber)
        {
            var deviceDto = GetDevice(serialNumber);
            if (deviceDto.IsWorking || deviceDto.Credit <= 0)
            {
                return false;
            }

            new Task(() => CountDown(serialNumber)).Start();
            return true;
        }


        public bool StopWork(int serialNumber)
        {
            var device = GetDevice(serialNumber);
            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                device = collection.FindOne(d => d.SerialNumber == serialNumber);
                device.IsInterrupted = true;
                device.IsWorking = false;
                collection.Update(device.Id, device);
            }
            return true;
        }

        private void CountDown(int serialNumber)
        {
            DeviceDto device;
            using (var db = new LiteDatabase(_databaseName))
            {
                var collection = db.GetCollection<DeviceDto>(_deviceTable);
                device = collection.FindOne(d => d.SerialNumber == serialNumber);
                device.IsWorking = true;
                device.IsInterrupted = false;
                collection.Update(device.Id, device);
            }
            while (device.Credit > 0 && !device.IsInterrupted)
            {
                using (var db = new LiteDatabase(_databaseName))
                {
                    var collection = db.GetCollection<DeviceDto>(_deviceTable);
                    var deviceTemp = device;
                    device = collection.FindOne(d => d.Id == deviceTemp.Id);
                    device.WorkCount = device.Credit > 10 ? device.WorkCount + 10 : device.WorkCount + device.Credit;
                    device.Credit = device.Credit > 10 ? device.Credit - 10 : 0;
                    if (device.Credit == 0)
                    {
                        device.IsWorking = false;
                        device.IsInterrupted = true;
                    }
                    collection.Update(device.Id, device);
                }
                Thread.Sleep(100);
                if (device.WorkCount % 100 <= 9)
                {
                    Console.WriteLine($"device {device.SerialNumber} is working, total work done {device.WorkCount} credit left {device.Credit}");
                }
            }

        }

    }

}
