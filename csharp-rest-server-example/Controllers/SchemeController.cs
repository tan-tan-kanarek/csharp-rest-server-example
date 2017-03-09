using ServerExample;
using ServerExample.App;
using ServerExample.Errors;
using ServerExample.Model;
using ServerExample.Model.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Routing;
using System.Web.Http.ValueProviders;
using System.Web.UI;
using System.Xml.Serialization;

namespace csharp_rest_server_example.Controllers
{
    public class SchemeController : ApiController
    {
        public class SchemeEnumValue
        {
            public SchemeEnumValue()
            {
            }

            public SchemeEnumValue(FieldInfo fieldInfo)
            {
                name = fieldInfo.Name;
                value = fieldInfo.GetRawConstantValue().ToString();
            }

            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string value;
        }

        public class SchemeEnum
        {
            public SchemeEnum()
            {
            }

            public SchemeEnum(Type type)
            {
                name = type.Name;

                foreach (FieldInfo fieldInfo in type.GetFields())
                {
                    if ((fieldInfo.Attributes & FieldAttributes.Static) > 0)
                    {
                        values.Add(new SchemeEnumValue(fieldInfo));
                    }
                }
            }

            [XmlAttribute]
            public string name;

            [XmlAttribute]
            public string enumType = "int";

            [XmlElement("const")]
            public List<SchemeEnumValue> values = new List<SchemeEnumValue>();
        }

        public class SchemeEnums
        {
            public SchemeEnums()
            {
            }

            [XmlElement("enum")]
            public List<SchemeEnum> enums = new List<SchemeEnum>();
        }
	
	    public class SchemeNestedType
        {
            private Type _nestedType;

            public SchemeNestedType()
            {
            }

            public SchemeNestedType(Type nestedType)
            {
                _nestedType = nestedType;

                SetType(nestedType);
            }

            private void SetType(Type nestedType)
            {
                if (nestedType.IsEnum)
                {
                    type = "int";
                    enumType = nestedType.Name;
                }
                else if (nestedType.IsGenericType)
                {
                    if (nestedType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        SetType(Nullable.GetUnderlyingType(nestedType));
                    }
                    else if (nestedType.GetGenericArguments().Count() == 1) // list
                    {
                        type = "array";
                        arrayType = nestedType.GetGenericArguments()[0].Name;
                    }
                    else if (nestedType.GetGenericArguments().Count() == 2) // map
                    {
                        type = "map";
                        arrayType = nestedType.GetGenericArguments()[1].Name;
                    }
                }
                else if (nestedType == typeof(RestDateTime))
                {
                    type = "int";
                    isTime = true;
                }
                else if (nestedType == typeof(long) || nestedType == typeof(Int64))
                {
                    type = "bigint";
                }
                else if (nestedType == typeof(int) || nestedType == typeof(Int32))
                {
                    type = "int";
                }
                else
                {
                    type = nestedType.Name;
                }
            }

            [XmlAttribute]
		    public string type;
            
            [XmlAttribute]
            public string arrayType;

            [XmlAttribute]
            public string enumType;

            [XmlAttribute]
            public bool isTime = false;

            public IEnumerable<Type> GetEnums()
            {
                return SchemeController.GetEnums(_nestedType);
            }

            public IEnumerable<Type> GetTypes()
            {
                return SchemeController.GetTypes(_nestedType);
            }
	    }

	    public class SchemeActionResult : SchemeNestedType
	    {
            public SchemeActionResult()
                : base()
            {
            }

            public SchemeActionResult(Type actionResult)
                : base(actionResult)
            {
            }
	    }

	    public class SchemeArgument : SchemeNestedType
        {
            public SchemeArgument()
            {
            }

            public SchemeArgument(Type argumentType)
                : base(argumentType)
            {
            }

		    [XmlAttribute]
		    public string name;

		    [XmlAttribute]
		    public string description;

		    [XmlAttribute]
		    public int minLength;

		    [XmlAttribute]
		    public int maxLength;

		    [XmlAttribute]
		    public double minValue;

		    [XmlAttribute]
		    public double maxValue;
	    }

	    public class SchemeActionParam : SchemeArgument
	    {
            public SchemeActionParam()
            {
            }

            public SchemeActionParam(ParameterInfo parameterInfo)
                : base(parameterInfo.ParameterType)
            {
                optional = parameterInfo.IsOptional;

                if (parameterInfo.IsOptional)
                {
                    if (parameterInfo.DefaultValue == null)
                    {
                        defaultValue = "null";
                    }
                    else
                    {
                        defaultValue = parameterInfo.DefaultValue.ToString();
                    }
                }
            }

            [XmlAttribute]
		    public bool optional;
            
            [XmlAttribute]
		    public string defaultValue;
	    }

	    public class SchemeActionThrows
	    {
		    [XmlAttribute]
		    public string name;
	    }

	    public class SchemeAction
	    {
            public SchemeAction()
            {
            }

            public SchemeAction(MethodInfo methodInfo)
            {
                ActionNameAttribute actionNameAttribute = methodInfo.GetCustomAttribute<ActionNameAttribute>();
                if(actionNameAttribute != null)
                {
                    name = actionNameAttribute.Name;
                }
                else
                {
                    name = methodInfo.Name;
                }

                if (methodInfo.ReturnType != typeof(void))
                {
                    result = new SchemeActionResult(methodInfo.ReturnType);
                }

                foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
                {
                    parameters.Add(new SchemeActionParam(parameterInfo));
                }
            }

            [XmlAttribute]
		    public string name;
            
            [XmlAttribute]
		    public string description;
            
            [XmlAttribute]
		    public bool enableInMultiRequest = true;
            
            [XmlElement("param")]
            public List<SchemeActionParam> parameters = new List<SchemeActionParam>();
            
            [XmlElement("result")]
		    public SchemeActionResult result;

            [XmlElement("throws")]
            public List<SchemeActionThrows> thrown = new List<SchemeActionThrows>();

            public IEnumerable<Type> GetEnums()
            {
                List<Type> list = new List<Type>();

                if (result != null)
                {
                    list.AddRange(result.GetEnums());
                }

                foreach (SchemeActionParam parameter in parameters)
                {
                    list.AddRange(parameter.GetEnums());
                }

                return list;
            }

            public IEnumerable<Type> GetTypes()
            {
                List<Type> list = new List<Type>();

                if (result != null)
                {
                    list.AddRange(result.GetTypes());
                }

                foreach (SchemeActionParam parameter in parameters)
                {
                    list.AddRange(parameter.GetTypes());
                }

                return list;
            }
	    }

	    public class SchemeService
        {
            public SchemeService()
            {
            }

            public SchemeService(Type controller)
            {
                string controllerName = controller.Name.Replace("Controller", "");
                controllerName = controllerName.Substring(0, 1).ToLower() + controllerName.Substring(1);
                id = controllerName;
                name = controllerName;

                foreach(MethodInfo methodInfo in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if(methodInfo.GetCustomAttribute<NonActionAttribute>() == null)
                    {
                        actions.Add(new SchemeAction(methodInfo));
                    }
                }
            }

            [XmlAttribute]
		    public string id;

            [XmlAttribute]
		    public string name;

            [XmlElement("action")]
            public List<SchemeAction> actions = new List<SchemeAction>();
            
            public IEnumerable<Type> GetEnums()
            {
                List<Type> list = new List<Type>();
                foreach (SchemeAction action in actions)
                {
                    list.AddRange(action.GetEnums());
                }
                return list;
            }

            public IEnumerable<Type> GetTypes()
            {
                List<Type> list = new List<Type>();
                foreach (SchemeAction action in actions)
                {
                    list.AddRange(action.GetTypes());
                }
                return list;
            }
	    }

	    public class SchemeServices
	    {
            [XmlElement("service")]
		    public List<SchemeService> services = new List<SchemeService>();
	    }

	    public class SchemeTypeProperty : SchemeArgument
	    {
            public SchemeTypeProperty()
            {
            }

            public SchemeTypeProperty(PropertyInfo propertyInfo)
                : base(propertyInfo.PropertyType)
            {
                name = propertyInfo.Name;
            }

		    [XmlAttribute]
		    public bool readOnly;
		
		    [XmlAttribute]
		    public bool insertOnly;
		
		    [XmlAttribute]
		    public bool writeOnly;
	    }

	    public class SchemeType
        {
            public SchemeType()
            {
            }

            public SchemeType(Type type)
            {
                name = type.Name;

                if (type.BaseType != typeof(RestObject) && !type.BaseType.IsGenericType)
                {
                    baseType = type.BaseType.Name;
                }

                AddProperties(type);
            }

            private void AddProperties(Type type)
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    properties.Add(new SchemeTypeProperty(propertyInfo));
                }

                if (type.BaseType != typeof(RestObject))
                {
                    AddProperties(type.BaseType);
                }
            }

		    [XmlAttribute]
		    public string name;

		    [XmlAttribute("base")]
		    public string baseType;
		
		    [XmlAttribute]
		    public string description;
            
            [XmlElement("property")]
		    public List<SchemeTypeProperty> properties = new List<SchemeTypeProperty>();
	    }

	    public class SchemeClasses
	    {
            [XmlElement("class")]
		    public List<SchemeType> classes = new List<SchemeType>();
	    }

	    public class SchemeErrorParameter
	    {
		    [XmlAttribute]
		    public string name;
	    }

	    public class SchemeError
	    {
		    [XmlAttribute]
		    public string name;

		    [XmlAttribute]
		    public string code;

		    [XmlAttribute]
		    public string message;
            
            [XmlElement("parameter")]
		    public List<SchemeErrorParameter> parameters;
	    }

	    public class SchemeErrors
	    {
            [XmlElement("error")]
		    public List<SchemeError> errors;
	    }
        
	    [XmlRoot("xml", Namespace = "")]
        public class Scheme
        {
            public SchemeEnums enums = new SchemeEnums();

            public SchemeClasses classes = new SchemeClasses();

            public SchemeServices services = new SchemeServices();

            public SchemeErrors errors = new SchemeErrors();

            private void Add(Type type)
            {
                // TODO
            }

            public void Add<IRestController>(Type controller)
            {
                services.services.Add(new SchemeService(controller));

                List<Type> enumTypes = new List<Type>();
                List<Type> classTypes = new List<Type>();
                //List<Type> errorTypes = new List<Type>();

                foreach (SchemeService service in services.services)
                {
                    enumTypes.AddRange(service.GetEnums());
                    classTypes.AddRange(service.GetTypes());
                    //errorTypes.AddRange(service.GetErrors());
                }

                foreach (Type type in enumTypes.Distinct())
                {
                    enums.enums.Add(new SchemeEnum(type));
                }

                foreach (Type type in classTypes.Distinct())
                {
                    classes.classes.Add(new SchemeType(type));
                }

                //foreach (Type type in classTypes.Distinct())
                //{
                //    errors.errors.Add(new SchemeError(type));
                //}
            }
        }

        [NonAction]
        public static IEnumerable<Type> GetEnums(Type type, List<Type> types = null)
        {
            if (types == null)
            {
                types = new List<Type>();
            }

            if (type.IsGenericType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return GetEnums(Nullable.GetUnderlyingType(type), types);
                }
                return types;
            }

            List<Type> list = new List<Type>();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(RestDateTime))
            {
                return list;
            }

            if (type.IsEnum)
            {
                list.Add(type);
            }
            else if (!types.Contains(type))
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    list.AddRange(GetEnums(propertyInfo.PropertyType, types));
                }

                if (type.BaseType != typeof(RestObject))
                {
                    list.AddRange(GetEnums(type.BaseType, types));
                }
            }
            return list;
        }

        [NonAction]
        public static IEnumerable<Type> GetTypes(Type type, List<Type> list = null)
        {
            if (list == null)
            {
                list = new List<Type>();
            }

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return GetTypes(Nullable.GetUnderlyingType(type), list);
                }

                return list;
            }

            if (type.IsPrimitive || type == typeof(string) || type == typeof(RestDateTime))
            {
                return list;
            }

            if (typeof(RestObject).IsAssignableFrom(type) && !list.Contains(type))
            {
                list.Add(type);

                foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    list.AddRange(GetTypes(propertyInfo.PropertyType, list));
                }

                if (type.BaseType != typeof(RestObject))
                {
                    list.AddRange(GetTypes(type.BaseType, list));
                }
            }
            return list;
        }

        [HttpPost, HttpGet]
        public Scheme Handle()
        {
            Scheme scheme = new Scheme();

            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> controllers = assembly.GetTypes().Where(type => typeof(IRestController).IsAssignableFrom(type) && typeof(IRestController) != type);

            foreach (Type controller in controllers)
            {
                scheme.Add<IRestController>(controller);
            }

            return scheme;
        }
    }
}