using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    public class RestList<T>
    : List<T>, IXmlSerializable
    {
        private static List<Type> knownTypes = null;

        private static IEnumerable<Type> GetKnownTypes()
        {
            if (knownTypes == null)
            {
                knownTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(IRestObject).IsAssignableFrom(type) && !type.IsGenericType).ToList();
                knownTypes.Add(typeof(RestResponse));
            }

            return knownTypes;
        }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            //TODO: implement
            XmlSerializer keySerializer = new XmlSerializer(typeof(string));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(T));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                T value = (T)valueSerializer.Deserialize(reader);

                Add(value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("objectType");
            writer.WriteString("array");
            writer.WriteEndElement();

            foreach (T value in this)
            {
                writer.WriteStartElement("item");

                if (value is string)
                {
                    writer.WriteString(value.ToString());
                }
                else
                {
                    if (value is RestResponse)
                    {
                        writer.WriteStartElement("objectType");
                        writer.WriteString("RestResponse");
                        writer.WriteEndElement();
                    }

                    XmlObjectSerializer xmlSerializer = new DataContractSerializer(typeof(T), GetKnownTypes());
                    MemoryStream stream = new MemoryStream();
                    xmlSerializer.WriteObject(stream, value);
                    stream.Position = 0;
                    StreamReader streamReader = new StreamReader(stream);
                    string xml = streamReader.ReadToEnd();
                    
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

                    writer.WriteRaw(doc.DocumentElement.InnerXml);
                }

                writer.WriteEndElement();
            }
        }
        #endregion
    }
}