using System.Collections.Generic;
using Newtonsoft.Json;

namespace Atom.Data
{
    public class AtomIndexDefinition
    {
        [JsonIgnore]
        public string Name { get; set; }

        public bool Unique { get; set; }

        public bool Queryable { get; set; }

        public List<string> Columns { get; set; }

        public IEnumerable<AtomMemberInfo> GetIndexMembers(AtomModel atom)
        {
            foreach (var column in Columns)
            {
                yield return atom.Members[column];
            }
        }
    }
}