using csharp_rest_server_example.Controllers;
using ServerExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace ServerExample.App
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Attribute routing.
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/service/{controller}/action/{action}"
            );
            config.Routes.MapHttpRoute(
                name: "Multirequest",
                routeTemplate: "api/service/multirequest",
                defaults: new { Controller = "multirequest" }
            );
            config.Routes.MapHttpRoute(
                name: "Scheme",
                routeTemplate: "",
                defaults: new { Controller = "scheme" }
            );

            config.Filters.Add(new RequestParser());
            config.Filters.Add(new ErrorHandler());
            config.MessageHandlers.Add(new WrappingHandler());


            List<Type> knownTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => typeof(IRestObject).IsAssignableFrom(type) && !type.IsGenericType).ToList();
            knownTypes.Add(typeof(RestResponseList));

            //List<Type> knownTypes = new List<Type>() { typeof(Dictionary<string, object>) };
            XmlObjectSerializer xmlSerializer = new DataContractSerializer(typeof(RestResponse), knownTypes);
            config.Formatters.XmlFormatter.SetSerializer<RestResponse>(xmlSerializer);

            config.Formatters.XmlFormatter.SetSerializer<SchemeController.Scheme>(new XmlSerializer(typeof(SchemeController.Scheme)));

            config.Formatters.XmlFormatter.WriterSettings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
        }
    }
}