using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elsa.Serialization.Formatters
{
    public class XmlTokenFormatter : ITokenFormatter
    {
        public const string FormatName = SerializationFormats.Xml;
        public string Format => FormatName;
        public string ContentType => "application/xml";

        public string ToString(JObject token)
        {
            var document = JsonConvert.DeserializeXNode(token.ToString(), "workflow");
            return document.ToString();
        }

        public JObject FromString(string data)
        {
            var document = XDocument.Parse(data);
            var json = JsonConvert.SerializeXNode(document, Formatting.Indented, false);
            return JObject.Parse(json);
        }
    }
}