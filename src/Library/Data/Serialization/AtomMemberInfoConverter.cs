using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class AtomMemberInfoConverter : JsonCreationConverter<AtomMemberInfo>
    {
        protected override AtomMemberInfo Deserialize(JsonSerializer serializer, JToken token)
        {
            if (token.Type == JTokenType.String)
            {
                return new AtomMemberInfo
                {
                    Type = token.ToObject<string>()
                };
            }

            return base.Deserialize(serializer, token);
        }

        protected override AtomMemberInfo After(AtomMemberInfo data)
        {
            return data;
        }
    }
}
