using System.Dynamic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NLog;

namespace HttpResponder
{
    public class DynamicXmlAdapter : DynamicObject
    {
        private static readonly Logger Log = LogManager.GetLogger("XmlBody");
        private readonly XElement root;

        public DynamicXmlAdapter(XElement root)
        {
            this.root = root;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var elements = this.root.Elements().Where(x => x.Name.LocalName == binder.Name).ToList();

            if (elements.Any())
            {
                switch (elements.Count)
                {
                    case 1:
                        result = new DynamicXmlAdapter(elements[0]);
                        return true;
                    default:
                        result = elements.Select(x => new DynamicXmlAdapter(x)).ToList();
                        return true;
                }
            }

            var attribute = this.root.Attributes().SingleOrDefault(x => x.Name.LocalName == binder.Name);

            if (attribute != null)
            {
                result = attribute.Value;

                return true;
            }

            result = null;
            return false;
        }

        public override string ToString()
        {
            return this.root.Value;
        }

        public string XPath(string path)
        {
            return this.root.XPathSelectElement(path).Value;
        }
    }
}