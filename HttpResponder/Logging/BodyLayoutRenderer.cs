using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;

namespace HttpResponder.Logging
{
    [LayoutRenderer("body")]
    public class BodyLayoutRenderer : LayoutRenderer
    {
        [DefaultParameter]
        [RequiredParameter]
        public string Item { get; set; }

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            var body = (dynamic)logEvent.Properties["body"];

            var value = Handlebars.Handlebars.Compile("{{" + this.Item + "}}")(body);
            builder.Append(value);
        }
    }
}
