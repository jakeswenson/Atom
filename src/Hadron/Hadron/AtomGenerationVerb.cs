using CommandLine;

namespace HadronApplication
{
    public abstract class AtomGenerationVerb : Verb
    {
        [Option('a', "atoms", Required = true, HelpText = "the input atom file or folder containing atoms")]
        public string AtomsFolder { get; set; }

        [Option('o', "output-path", Required = true)]
        public string OutputPath { get; set; }
    }
}