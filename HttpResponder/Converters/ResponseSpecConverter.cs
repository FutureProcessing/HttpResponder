using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpResponder.Converters {
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ResponseSpecConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.ReadFrom(reader);
            var obj = token as JObject;
            if (obj == null)
            {
                throw new InvalidOperationException("Expected object in JSON. Got: " + token);
            }

            if (obj.Property("script") == null)
            {                
                return obj.ToObject<ResponseSpec>(serializer);
            }

            return obj.ToObject<ScriptResponseSpec>(serializer);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (IResponseSpec);
        }
    }
}
