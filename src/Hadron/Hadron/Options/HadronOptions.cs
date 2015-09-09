using CommandLine;
using CommandLine.Text;
using HadronApplication.Options.Verbs;
using HadronApplication.Options.Verbs.AtomGenerators;

namespace HadronApplication.Options
{
    public class HadronOptions
    {
        public HadronOptions()
        {
        }

        [VerbOption("sql", HelpText = "Configuration and enabling of generating sql access")]
        public SqlVerb SqlVerb { get; set; }

        [VerbOption("csharp", HelpText = "Configuration and enabling of generating c# access")]
        public CSharpVerb CSharpVerb { get; set; }

        [VerbOption("fromdb", HelpText = "Builds .atom files from a sql server schema")]
        public ReverseFromDbAtomBuilderVerb ReverseFromDbVerb { get; set; }

        [VerbOption("sample", HelpText = "Outputs a sample atom")]
        public SampleVerb SampleVerb { get; set; }

        [VerbOption("gen", HelpText = "Runs hadron from the local .atom.config file")]
        public GenerationVerb GenFromConfig { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }
}
