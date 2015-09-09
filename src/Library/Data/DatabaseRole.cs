using Newtonsoft.Json;
using Atom.Data.Serialization;

namespace Atom.Data
{
    [JsonConverter(typeof(DatabaseRoleConverter))]
    public class DatabaseRole
    {
        public string Name { get; set; }
    }
}
