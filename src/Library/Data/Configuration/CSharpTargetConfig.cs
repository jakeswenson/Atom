using Newtonsoft.Json;

namespace Atom.Data
{
    public class CSharpTargetConfig : TargetConfig
    {
        public CSharpTargetConfig()
        {
            Entities = new EntitiesTargetConfiguration();
            Repository = new RepositoryTargetConfiguration();
            FileExtension = ".cs";
        }

        public string FileExtension { get; set; }

        [JsonProperty("repo")]
        public RepositoryTargetConfiguration Repository { get; set; }

        public EntitiesTargetConfiguration Entities { get; set; }
    }
}
