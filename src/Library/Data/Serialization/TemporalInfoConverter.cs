using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class TemporalInfoConverter : JsonCreationConverter<TemporalInfo>
    {
        protected override TemporalInfo Deserialize(JsonSerializer serializer, JToken token)
        {
            if (token.Type == JTokenType.Boolean)
            {
                return new TemporalInfo
                       {
                           HasTemporal = token.ToObject<bool>()
                       };
            }

            return base.Deserialize(serializer, token);
        }

        protected override TemporalInfo After(TemporalInfo data)
        {
            return data;
        }
    }
}
