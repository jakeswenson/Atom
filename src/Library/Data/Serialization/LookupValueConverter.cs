using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class LookupValueConverter : JsonCreationConverter<LookupValue>
    {
        protected override LookupValue Deserialize(JsonSerializer serializer, JToken token)
        {
            if (token.Type == JTokenType.Array)
            {
                var values = token.ToObject<List<JToken>>();
                if (values.Count >= 3 && values[0].Type == JTokenType.Integer)
                {
                    var hasDeletedValue = values.Count >= 4 && values[3].Type == JTokenType.Boolean;
                    return new LookupValue
                    {
                        Index = values[0].Value<int>(),
                        Name = values[1].Value<string>(),
                        Description = values[2].Value<string>(),
                        IsDeleted = hasDeletedValue ? values[3].Value<bool>() : false,
                        OtherColumns = values.Skip(hasDeletedValue ? 4 : 3).Select(t => t.Value<string>()).ToList(),
                    };
                }
                
                if (values.Count >= 2)
                {
                    var hasDeletedValue = values.Count >= 3 && values[2].Type == JTokenType.Boolean;
                    return new LookupValue
                    {
                        Name = values[0].Value<string>(),
                        Description = values[1].Value<string>(),
                        IsDeleted = hasDeletedValue ? values[2].Value<bool>() : false,
                        OtherColumns = values.Skip(hasDeletedValue? 3 : 2).Select(t => t.Value<string>()).ToList()
                    };
                }
            }

            return base.Deserialize(serializer, token);
        }

        protected override LookupValue After(LookupValue data)
        {
            return data;
        }
    }
}
