using ClassCreator.Controllers.Core;
using ClassCreator.Data.Common;
using ClassCreator.Data.Files.Core;
using ClassCreator.Data.Files.DTO;
using System.Reflection;

namespace ClassCreator.Main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = (WebApplication)CreateHost(args);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static IHost CreateHost(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var services = builder.Services;
            services
                .AddTransient<IObjectHandler<FileObjectDataDto>, ObjectFileHandler>()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddLogging()
                .AddOptions()
                .AddMvc().AddApplicationPart(Assembly.GetAssembly(typeof(ObjectFileController))).AddControllersAsServices();

            return builder.Build();
        }
    }
}