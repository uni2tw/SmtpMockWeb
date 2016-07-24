using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace SmtpMockWeb.Code
{

    public interface ISierializeContractResolver
    {
        IContractResolver ContractResolver { get; }
    }

    public class JsonUtil
    {
        public static T Deserialize<T>(string jsonString, params JsonConverter[] converters)
        {
            if (String.IsNullOrEmpty(jsonString))
            {
                return default(T);
            }
            if (converters == null || converters.Length == 0)
            {
                converters = new JsonConverter[]
                {
                    new IsoDateTimeConverter()
                };
            }
            return JsonConvert.DeserializeObject<T>(jsonString, converters);
        }

        public static object Deserialize(Type type, string jsonString, params JsonConverter[] converters)
        {
            if (String.IsNullOrEmpty(jsonString))
            {
                return null;
            }
            if (converters == null || converters.Length == 0)
            {
                converters = new JsonConverter[]
                {
                    new IsoDateTimeConverter()
                };
            }
            return JsonConvert.DeserializeObject(jsonString, type, converters);
        }

        public static dynamic ParseDynamic(string json)
        {
            return JObject.Parse(json);
        }

        public static string Serialize(object obj)
        {
            return Serialize(obj, null, true);
        }

        public static string Serialize(object obj, ISierializeContractResolver resolver, bool indent)
        {
            JsonSerializer json;
            if (resolver == null)
            {
                json = new JsonSerializer();
            }
            else
            {
                json = new JsonSerializer()
                {
                    ContractResolver = resolver.ContractResolver
                };
            }

            json.NullValueHandling = NullValueHandling.Include;

            json.ObjectCreationHandling = ObjectCreationHandling.Replace;
            json.MissingMemberHandling = MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            //json.Converters.Add(new IsoDateTimeConverter());
            json.Converters.Add(new JavaScriptDateTimeConverter());

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.Formatting = Formatting.Indented;

            writer.QuoteChar = '"';
            json.Serialize(writer, obj);


            string output = sw.ToString();
            writer.Close();
            sw.Close();

            return output;
        }
    }
}