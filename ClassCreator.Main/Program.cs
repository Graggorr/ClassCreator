using ClassCreator.Data.Common;
using ClassCreator.Data.Core;
using System.Reflection;
using System.Xml;

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
                .AddLogging()
                .AddSingleton<IObjectHandler, ObjectHandler>()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen()
                .AddMvcCore();

            return builder.Build();
        }

        private static void PrepareAssembly()
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.CreateElement("1");
        }
    }
}