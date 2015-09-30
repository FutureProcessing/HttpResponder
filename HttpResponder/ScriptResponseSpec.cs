namespace HttpResponder
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Jint;
    using Jint.Native.Json;
    using Microsoft.Owin;
    using Newtonsoft.Json.Linq;
    using NLog;

    public class ScriptResponseSpec : IResponseSpec
    {
        public string Script { get; set; }

        public async Task Render(IOwinRequest request, IOwinResponse response, LogFactory requestLogging)
        {
            var engine = new Engine(opts => opts.AllowClr(AppDomain.CurrentDomain.GetAssemblies()));

            var scriptBody = File.ReadAllText(this.Script);

            engine.Execute(scriptBody);

            try
            {
                var spec = engine.Invoke("spec", request.Environment["body"], request);

                var specJson = JObject.FromObject(spec.ToObject());

                var responseSpec = specJson.ToObject<IResponseSpec>(ConfigJson.Serializer);

                await responseSpec.Render(request, response, requestLogging);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}