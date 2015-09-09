using System.Collections.Generic;
using Atom.Data.Projections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class ProjectionInfoConverter : JsonCreationConverter<ProjectionInfo>
    {
        protected override ProjectionInfo After(ProjectionInfo data)
        {
            return data;
        }

        protected override ProjectionInfo Deserialize(JsonSerializer serializer, JToken token)
        {
            if (token.Type == JTokenType.Array)
            {
                return new ProjectionInfo
                {
                    SelectMembers = token.ToObject<List<string>>()
                };
            }

            return base.Deserialize(serializer, token);
        }
    }
}
