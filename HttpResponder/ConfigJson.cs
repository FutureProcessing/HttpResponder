using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpResponder {
    using System.IO;
    using Converters;
    using Newtonsoft.Json;

    public static class ConfigJson {
        public static JsonSerializer Serializer { get; private set; }

        static ConfigJson()
        {
            Serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                Converters =
                {
                    new ResponseSpecConverter()
                }
            });
        }

        public static T Deserialize<T>(string text)
        {
            using (var stringReader = new StringReader(text))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}
