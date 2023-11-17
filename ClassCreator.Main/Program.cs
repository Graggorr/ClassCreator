using ClassCreator.Controllers.Core;
using ClassCreator.Data.Common;
using ClassCreator.Data.Core;
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
                .AddTransient<IObjectHandler, ObjectHandler>()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddLogging()
                .AddOptions()
                .AddMvc().AddApplicationPart(Assembly.GetAssembly(typeof(ObjectController))).AddControllersAsServices();

            return builder.Build();
        }
    }
}