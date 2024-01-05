using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using TrendNET.WMS.Device.App;

namespace TrendNET.WMS.Core.Data
{
    public class CompactSerializer
    {
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        public static string Serialize<T>(T value)
        {
            var startedAt = DateTime.Now;
            try
            {
                if (value == null)
                {
                    return null;
                }

                XmlSerializer serializer;
                if (serializers.ContainsKey(typeof(T)))
                {
                    serializer = serializers[typeof(T)];
                }
                else
                {
                    serializer = new XmlSerializer(typeof(T));
                    serializers.Add(typeof(T), serializer);
                }
                
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = new UTF8Encoding(false, false);
                settings.Indent = false;
                settings.OmitXmlDeclaration = false;
                using (StringWriter textWriter = new StringWriter())
                {
                    using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                    {
                        serializer.Serialize(xmlWriter, value);
                    }
                    return textWriter.ToString();
                }
            }
            finally
            {
                Log.Write(new LogEntry("END REQUEST: [Device/CompactSerializer.Serialize];" + (DateTime.Now - startedAt).TotalMilliseconds.ToString()));
            }
        }




        public static T Deserialize<T>(string xml)
        {
            var startedAt = DateTime.Now;
            try
            {
                if (string.IsNullOrEmpty(xml))
                {
                    return default(T);
                }

                XmlSerializer serializer;
                if (serializers.ContainsKey(typeof(T)))
                {
                    serializer = serializers [typeof(T)];
                } else {
                    serializer = new XmlSerializer(typeof(T));
                    serializers.Add (typeof(T), serializer);
                }                 

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.CheckCharacters = false;
                using (StringReader textReader = new StringReader(xml))
                {
                    using (XmlReader xmlReader = XmlReader.Create(textReader, settings))
                    {
                        return (T)serializer.Deserialize(xmlReader);
                    }
                }
            } 
            finally
            {
                Log.Write(new LogEntry("END REQUEST: [Device/CompactSerializer.Deserialize];" + (DateTime.Now - startedAt).TotalMilliseconds.ToString()));
            }
        }
    }
}
