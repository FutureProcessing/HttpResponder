namespace HttpResponder
{
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using NLog;

    public interface IResponseSpec
    {
        Task Render(IOwinRequest request, IOwinResponse response, LogFactory requestLogging);
    }
}