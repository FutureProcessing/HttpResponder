namespace HttpResponder
{
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using NLog;

    public class NotFoundResponse : IResponseSpec
    {
        public async Task Render(IOwinRequest request, IOwinResponse response, LogFactory requestLogging)
        {
            response.StatusCode = 404;
            response.ReasonPhrase = "Not found";
            await response.WriteAsync(string.Format("Not found {0} {1}", request.Method, request.Path));
        }
    }
}