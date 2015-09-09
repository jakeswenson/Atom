using System.Collections.Generic;
using Newtonsoft.Json;
using Atom.Data;

namespace HadronApplication.Options
{
    public class AtomConfig
    {
        public string AtomPath { get; set; }

        [JsonConverter(typeof(DictionaryTargetConverter))]
        public Dictionary<string, TargetConfig> Targets { get; set; }
    }
}
