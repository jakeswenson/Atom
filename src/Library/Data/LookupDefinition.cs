using System.Collections.Generic;

namespace Atom.Data
{
    public class LookupDefinition
    {
        public string NamePrefix { get; set; }
        public string Type { get; set; }
        public List<LookupValue> Values { get; set; }
    }
}