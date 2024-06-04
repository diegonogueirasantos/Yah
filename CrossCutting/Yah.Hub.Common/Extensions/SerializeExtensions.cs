using System.Text;
using System.Xml.Serialization;
using System.Xml;

namespace Yah.Hub.Common.Extensions
{
    public class SerializeExtensions
    {
        public static string Serialize(object rootObject, bool indent = false)
        {
            var type = rootObject.GetType();
            var serializer = new XmlSerializer(type);
            string xml = null;

            using (var sww = new Utf8StringWriter())
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = indent,
                    IndentChars = "\t",
                    NewLineChars = "\r\n",
                    NewLineHandling = NewLineHandling.Replace
                };

                using (XmlWriter writer = XmlWriter.Create(sww, settings))
                {
                    serializer.Serialize(writer, rootObject);
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        private sealed class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
