using System.IO;
using Newtonsoft.Json;

namespace Atom.Data
{
    public abstract class TargetConfig
    {
    }

    public abstract class OutputTargetConfiguration : TargetConfig
    {
        [JsonProperty("path")]
        public string OutputPath { get; set; }

        public virtual void MakeAbsolutePath(string baseDir)
        {
            OutputPath = Path.GetFullPath(Path.Combine(baseDir, OutputPath));
        }
    }

    public abstract class NamespacedOutputTargetConfiguration : OutputTargetConfiguration
    {
        public string Namespace { get; set; }
    }
}
