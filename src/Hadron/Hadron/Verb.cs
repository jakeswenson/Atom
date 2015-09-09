using System.Collections.Generic;
using System.IO;
using CommandLine;
using HadronApplication.Options;
using HadronApplication.Options.Verbs;
using Newtonsoft.Json;
using Atom.Data;
using Atom.Generation;
using Serilog;
using System.Linq;

namespace HadronApplication
{
    public abstract class Verb
    {
        public static IEnumerable<Verb> Parse(string[] args, HadronOptions options)
        {
            var verbs = new List<Verb>();
            if (Parser.Default.ParseArguments(args, options, (s, o) => verbs.Add((Verb)o)))
            {
                return verbs;
            }

            return null;
        }

        public void Run()
        {
            foreach (var item in Generate())
            {
                WriteResults(item);
            }
        }

        protected abstract IEnumerable<GeneratorResult> Generate();

        protected void WriteResults(GeneratorResult result)
        {
            Directory.CreateDirectory(result.OutputPath);

            foreach (var delete in result.Deletions)
            {
                if (!File.Exists(delete)) { continue; }

                Log.Debug("Deleting file: {File}", delete);

                File.Delete(delete);
            }

            foreach (var generatorOutput in result.Outputs)
            {
                var path = Path.GetFullPath(Path.Combine(result.OutputPath, generatorOutput.FileName));

                var folder = Path.GetDirectoryName(path);

                Directory.CreateDirectory(folder);

                Log.Information("Writing output {FileName} to: {Path}", generatorOutput.FileName, path);

                File.WriteAllText(path, contents: generatorOutput.Output.Trim());
            }
        }
    }
}
