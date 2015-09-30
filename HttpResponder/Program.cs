using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Fluent;
using Owin;

namespace HttpResponder
{
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Config;
    using Converters;
    using Microsoft.Owin;
    using Newtonsoft.Json.Linq;

    class Program
	{
		private static readonly Logger Log = LogManager.GetLogger("Startup");
		private static readonly Logger Http = LogManager.GetLogger("HTTP");

		static void Main(string[] args)
		{
			var options = new StartOptions(args[0]);

			var configFile = args[1];

			LogFactory requestLogging;

			if (args.Length == 3)
			{
				var xmlLoggingConfiguration = new XmlLoggingConfiguration(args[2]) { AutoReload = true };

				requestLogging = new LogFactory(xmlLoggingConfiguration);
			}
			else
			{
				requestLogging = new LogFactory(new LoggingConfiguration());
			}

			Action<IAppBuilder> appBuilder = app => ConfigureHttp(app, configFile, requestLogging);

			using (WebApp.Start(options, appBuilder))
			{
				Log.Info("HTTP server ready on {0}. Press [Enter] to stop", options.Urls[0]);
				Console.ReadLine();
			}
		}

		private static void ConfigureHttp(IAppBuilder app, string configFile, LogFactory requestLogging)
		{
			app.UseErrorPage(new ErrorPageOptions 
			{
				ShowExceptionDetails = true
			});

		    app.Use(async (ctx, next) =>
		    {
		        DecodeBody(ctx);

		        await next();
		    });

            app.Run(async ctx =>
            {
                var config = ConfigJson.Deserialize<Configuration>(File.ReadAllText(configFile));                

                Http.Info("Incoming request {0} {1}", ctx.Request.Method, ctx.Request.Path);                

                var responseSpec = config.FindMatching(ctx.Request.Method, ctx.Request.Path.ToString()) ?? new NotFoundResponse();

                await responseSpec.Render(ctx.Request, ctx.Response, requestLogging);
            });
        }

        private static void DecodeBody(IOwinContext ctx)
        {
            ctx.Request.Environment["body"] = ReadBody(ctx.Request);
        }

        private static dynamic ReadBody(IOwinRequest request) {
            dynamic body = null;
            string stringBody = "";

            if (request.Method != "GET") {

                using (var sr = new StreamReader(request.Body)) {
                    stringBody = sr.ReadToEnd();
                }

                var contentType = request.ContentType.Split(';').First();
                switch (contentType.ToLower()) {
                    case "application/json":
                        body = ReadJsonInput(stringBody);
                        break;
                    case "text/xml":
                        body = ReadXmlInput(stringBody);
                        break;
                }
            }

            request.Environment["raw-body"] = stringBody;

            return body;
        }

        private static dynamic ReadXmlInput(string body) {
            var xml = XDocument.Parse(body);
            return new DynamicXmlAdapter(xml.Root);
        }

        private static dynamic ReadJsonInput(string body) {
            return JToken.Parse(body);
        }
	}
}
