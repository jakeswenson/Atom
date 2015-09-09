using System.Collections.Generic;
using CommandLine;
using Atom.Data;
using Atom.Generation;
using Atom.Generation.Generators.AtomGenerator;

namespace HadronApplication.Options.Verbs
{
    public class ReverseFromDbAtomBuilderVerb : Verb
    {
        [Option('o', Required = true, HelpText = "The output location for generated .atom files")]
        public string Output { get; set; }

        [Option('h', Required = true, HelpText = "The host that has the sql server schema")]
        public string Host { get; set; }

        [Option('d', Required = true, HelpText = "The database name on the host")]
        public string DatabaseName { get; set; }

        [Option("inferHiddenKey", HelpText = "If set will use surrogate keys for all SPROCS if it finds them based on whether the key name follows the pattern {TableName}Guid")]
        public bool InferrHiddenKey { get; set; }

        protected override IEnumerable<GeneratorResult> Generate()
        {
            var result = new AtomGenerator().Generate(
                new GeneratorArguments<AtomGenerationTargetConfig>(                    
                    config: new AtomGenerationTargetConfig
                    {
                        Database = DatabaseName,
                        Host = Host,
                        InferrHiddenKey = InferrHiddenKey,
                        OutputPath = Output
                    },
                    atomsFolder: null,
                    defaults: null));

            result.OutputPath = Output;

            yield return result;
        }
    }
}
