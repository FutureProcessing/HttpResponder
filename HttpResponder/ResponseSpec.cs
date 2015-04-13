using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;

namespace HttpResponder
{
    public class ResponseSpec
    {
        public string Response { get; set; }
        public string Template { get; set; }

        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; }
        public string ContentType { get; set; }
        public TimeSpan? Sleep { get; set; }
        public string LogUsing { get; set; }

        public Dictionary<string, object> Headers { get; set; }

        public ResponseSpec()
        {
            this.StatusCode = 200;
            this.ReasonPhrase = "OK";
            this.ContentType = "text/plain";
            this.Headers = new Dictionary<string, object>();
        }

        public virtual async Task Render(IOwinRequest request, IOwinResponse response, LogFactory requestLogging)
        {
            response.StatusCode = this.StatusCode;
            response.ReasonPhrase = this.ReasonPhrase;
            response.ContentType = this.ContentType;

            dynamic body = await ReadBody(request);

            LogResponse(request, requestLogging, body);

            DoSleep();

            WriteHeaders(response);
            
            await WriteResponse(request, response, body);                      
        }

        private void LogResponse(IOwinRequest request, LogFactory requestLogging, dynamic body)
        {
            if (string.IsNullOrWhiteSpace(this.LogUsing))
            {
                return;
            }                       

            var logger = requestLogging.GetLogger(this.LogUsing);
       
            var eventInfo = new LogEventInfo(LogLevel.Trace, this.LogUsing, "");
            eventInfo.Properties["body"] = body;

            logger.Log(eventInfo);
        }

        private async Task WriteResponse(IOwinRequest request, IOwinResponse response, dynamic body)
        {
            if (this.Response != null)
            {
                await response.WriteAsync(this.Response);
            }
            else if (this.Template != null)
            {
                var tmpl = Handlebars.Handlebars.Compile(File.ReadAllText(this.Template));

                var text = tmpl(body);

                await response.WriteAsync(text);
            }
        }

        private static async Task<dynamic> ReadBody(IOwinRequest request)
        {
            dynamic body = null;

            if (request.Method != "GET")
            {
                switch (request.ContentType.ToLower())
                {
                    case "application/json":
                        body = ReadJsonInput(request.Body);
                        break;
                    case "text/xml":
                        body = await ReadXmlInput(request);
                        break;
                }
            }
            return body;
        }

        private void WriteHeaders(IOwinResponse response)
        {
            foreach (var header in this.Headers)
            {
                if (header.Value is JArray)
                {
                    var values = ((JArray) header.Value).Select(x => (string) x).ToArray();
                    response.Headers.Add(header.Key, values);
                }
                else
                {
                    response.Headers.Append(header.Key, header.Value.ToString());
                }
            }
        }

        private void DoSleep()
        {
            if (this.Sleep.HasValue)
            {
                Thread.Sleep((TimeSpan) this.Sleep.Value);
            }
        }

        private static async Task<dynamic> ReadXmlInput(IOwinRequest request)
        {            
            using (var sr = new StreamReader(request.Body))
            {
                var xml = XDocument.Parse(await sr.ReadToEndAsync());
                return new DynamicXmlAdapter(xml.Root);
            }            
        }

        private static dynamic ReadJsonInput(Stream bodyStream)
        {
            JObject obj;

            using (var reader = new StreamReader(bodyStream))
            {
                using (var jsonReader = new JsonTextReader(reader))
                {
                    obj = (JObject)JToken.ReadFrom(jsonReader);
                }
            }
            return obj;
        }
    }
}