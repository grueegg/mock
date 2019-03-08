using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

namespace IoTApiMock.Exceptions
{
    public class HttpGlobalNotFoundExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            ProblemDetails error = null; 
            
            var exceptionType = context.Exception.GetType();

            if (exceptionType == typeof(DeviceNotFoundException))
            {
                error = new ProblemDetails
                {
                    Type = "NotFound",
                    Title = "Device could not be found",
                    Status = StatusCodes.Status404NotFound,
                    
                };
            }
            if (error.Status != null) context.HttpContext.Response.StatusCode = (int)error.Status;
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(error)).Wait();
            context.ExceptionHandled = true;
        }
    }
}
