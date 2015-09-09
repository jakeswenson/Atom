namespace Atom.Data
{
    public class AtomGenerationTargetConfig : OutputTargetConfiguration
    {
        public string Database { get; set; }

        public string Host { get; set; }

        public bool InferrHiddenKey { get; set; }
    }
}
