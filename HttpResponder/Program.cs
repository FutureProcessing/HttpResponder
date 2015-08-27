using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using NLog;
using NLog.Config;
using NLog.Fluent;
using Owin;

namespace HttpResponder
{
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

            app.Run(async ctx =>
            {
                var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(configFile));                

                Http.Info("Incoming request {0} {1}", ctx.Request.Method, ctx.Request.Path);                

                var responseSpec = config.FindMatching(ctx.Request.Method, ctx.Request.Path.ToString()) ?? new NotFoundResponse();

                await responseSpec.Render(ctx.Request, ctx.Response, requestLogging);
            });
        }
    }

    public class Configuration : Dictionary<string, MethodHandling>
    {
        public ResponseSpec FindMatching(string method, string path)
        {
            if (this.ContainsKey(method.ToLower()))
            {
                return this[method.ToLower()].FindMatching(path);
            }

            if (this.ContainsKey("*"))
            {
                return this["*"].FindMatching(path);
            }

            return null;
        }
    }

    public class MethodHandling : Dictionary<string, ResponseSpec>
    {
        public ResponseSpec FindMatching(string path)
        {
            if (this.ContainsKey(path))
            {
                return this[path];
            }

            if (this.ContainsKey("*"))
            {
                return this["*"];
            }

            return null;
        }
    }

    public class NotFoundResponse : ResponseSpec
    {
        public override async Task Render(IOwinRequest request, IOwinResponse response, LogFactory requestLogging)
        {
            response.StatusCode = 404;
            response.ReasonPhrase = "Not found";
            await response.WriteAsync(string.Format("Not found {0} {1}", request.Method, request.Path));
        }
    }
}
