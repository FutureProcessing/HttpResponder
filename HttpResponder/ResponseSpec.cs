using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using NLog.Config;

namespace HttpResponder
{
    public class ResponseSpec : IResponseSpec
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

            dynamic body = request.Environment["body"];

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
            eventInfo.Properties["raw-body"] = request.Environment["raw-body"];

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
    }
}