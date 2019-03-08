using IoTApiMock.DTO;
using LiteDB;
using Microsoft.AspNetCore.Builder;

namespace IoTApiMock
{
    public static class DatabaseInInitiatorExtensions
    {
        public static void UseDatabaseInInitiator(this IApplicationBuilder app)
        {
            //used to reset all running devices on startup.
            //devices could be on state running without a running instance, because of a previous hard shutdown of the application
            using (var db = new LiteDatabase("device.db"))
            {
                var collection = db.GetCollection<DeviceDto>("devices");
                var devices = collection.FindAll();
                foreach (var device in devices)
                {
                    device.IsInterrupted = true;
                    device.IsWorking = false;
                    collection.Update(device);
                }
            }
        }
    }
}
