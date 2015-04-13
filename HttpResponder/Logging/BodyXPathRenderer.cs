using System.Text;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace HttpResponder.Logging
{
    [LayoutRenderer("bodyxml")]
    public class BodyXPathRenderer : LayoutRenderer
    {
        [DefaultParameter]
        [RequiredParameter]
        public string Path { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var xmlBody = logEvent.Properties["body"] as DynamicXmlAdapter;

            if (xmlBody != null)
            {
                builder.Append(xmlBody.XPath(this.Path));
            }
            else
            {
                builder.Append("<NO XML>");
            }
        }
    }
}