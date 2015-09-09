using Newtonsoft.Json;
using Atom.Data.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Atom.Data
{
    [JsonConverter(typeof(LookupValueConverter))]
    public class LookupValue
    {
        public LookupValue()
        {
            OtherColumns = new List<string>();
        }

        public int? Index { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> OtherColumns { get; internal set; }

        public bool IsDeleted { get; set; }
    }
}
