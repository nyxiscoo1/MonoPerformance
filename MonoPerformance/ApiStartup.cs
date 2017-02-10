using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Service.Middleware.RequestLogger;

namespace MonoPerformance
{
    internal class ApiStartup
    {
        // This code configures Web API. The ApiStartup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute("Report", "api/v1/{action}", new { controller = "Api", action = "about" });

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.FloatParseHandling = FloatParseHandling.Decimal;
            json.SerializerSettings.Formatting = Formatting.Indented;
            json.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            json.SerializerSettings.DateParseHandling = DateParseHandling.DateTime;
            json.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            json.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            json.SerializerSettings.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            JsonConvert.DefaultSettings = () => json.SerializerSettings;

            appBuilder
                //.UseRequestLogger()
                .UseWebApi(config);
        }
    }
}