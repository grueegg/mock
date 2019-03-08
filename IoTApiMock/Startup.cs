using System;
using System.IO;
using System.Reflection;
using AutoMapper;
using IoTApiMock.Exceptions;
using IoTApiMock.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Swagger;


namespace IoTApiMock
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "Hackathon Device Mock",
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
            services.AddMemoryCache();
            services.AddAutoMapper();
            services.AddMvc(options => options.Filters.Add(typeof(HttpGlobalNotFoundExceptionFilter)));
            services.AddTransient<DeviceService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
           

            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
            }
            app.UseDatabaseInInitiator();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "gui")),
                RequestPath = "/gui"
            });

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoTApiMock");
            });

            app.UseMvc();

        }
    }
}
