using System.Collections.Generic;
using Atom.Data.Serialization;
using Newtonsoft.Json;

namespace Atom.Data.Projections
{
    [JsonConverter(typeof(ProjectionInfoConverter))]
    public class ProjectionInfo
    {
        public ProjectionInfo()
        {
            SelectMembers = new List<string>();
        }

        [JsonProperty("members")]
        public List<string> SelectMembers { get; set; }

        public Dictionary<string, string> Aliases { get; set; }
    }
}
