using Newtonsoft.Json;

namespace Atom.Data
{
    public class AtomGroupDefinition
    {
        public bool Unique { get; set; }

        [JsonIgnore]
        internal string Name { get; set; }
    }
}
