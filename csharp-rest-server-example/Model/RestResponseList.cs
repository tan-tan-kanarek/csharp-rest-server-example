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
    public class RestResponseList : RestList<RestResponse>
    {
    }
}