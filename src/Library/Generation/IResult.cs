using System.Collections;
using System.Collections.Generic;
using Atom.Generation.Data;

namespace Atom.Generation
{
    public class GeneratorResult : IEnumerable<GeneratorOutput>
    {
        private readonly List<GeneratorOutput> _outputs = new List<GeneratorOutput>();

        public string OutputPath { get; set; }

        public void AddOutput(string fileName, string output)
        {
            _outputs.Add(new GeneratorOutput(fileName, output));
        }

        public void AddOutput(GeneratorOutput generatorOutput)
        {
            _outputs.Add(generatorOutput);
        }

        public virtual IEnumerable<GeneratorOutput> Outputs
        {
            get
            {
                return _outputs.AsReadOnly();
            }
        }

        public List<string> Deletions { get; } = new List<string>();

        public IEnumerator<GeneratorOutput> GetEnumerator()
        {
            return Outputs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
