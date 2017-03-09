using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ServerExample.Model
{
    public class RestDateTime : IXmlSerializable
    {
        public DateTime Value { get; set; }

        public RestDateTime()
        {
        }

        public RestDateTime(DateTime value)
        {
            Value = value;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                reader.ReadStartElement();
                return;
            }

            string longString = reader.ReadInnerXml();
            if (String.IsNullOrWhiteSpace(longString) == false)
            {
                long seconds = XmlConvert.ToInt64(longString);
                Value = RestDatabase.DateTimeFromTimestamp(seconds);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (Value == null)
                return;

            writer.WriteRaw(XmlConvert.ToString(RestDatabase.DateTimeToTimestamp(Value)));
        }
    }
}