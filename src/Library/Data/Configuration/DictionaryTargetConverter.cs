using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Atom.Data;
using Atom.Data.Serialization;

namespace HadronApplication.Options
{
    public class DictionaryTargetConverter : JsonCreationConverter<Dictionary<string, TargetConfig>>
    {
        private readonly Dictionary<string, Type> _serializerMap = new Dictionary<string, Type>
        {
            { "c#", typeof(CSharpTargetConfig) },
            { "sql", typeof(SqlTargetConfig) }
        };

        protected override Dictionary<string, TargetConfig> Deserialize(JsonSerializer serializer, JToken token)
        {
            var dict = new Dictionary<string, TargetConfig>();

            foreach (var key in ((JObject)token).Properties()
                                                  .Select(p => p.Name))
            {
                var value = ((JObject)token).Property(key).Value;

                if (_serializerMap.ContainsKey(key))
                {
                    dict[key] = JsonConvert.DeserializeObject(value.ToString(), _serializerMap[key]) as TargetConfig;
                }
            }

            return dict;
        }

        protected override Dictionary<string, TargetConfig> After(Dictionary<string, TargetConfig> data)
        {
            return data;
        }
    }
}
