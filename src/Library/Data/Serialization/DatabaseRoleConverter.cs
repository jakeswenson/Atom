using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class DatabaseRoleConverter : JsonCreationConverter<DatabaseRole>
    {
        protected override DatabaseRole Deserialize(JsonSerializer serializer, JToken token)
        {
            if (token.Type == JTokenType.String)
            {
                return new DatabaseRole
                {
                    Name = token.ToObject<string>()
                };
            }

            return base.Deserialize(serializer, token);
        }

        protected override DatabaseRole After(DatabaseRole data)
        {
            return data;
        }
    }
}
