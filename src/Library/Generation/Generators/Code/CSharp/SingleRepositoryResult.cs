using System.Collections.Generic;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class SingleRepositoryResult
    {
        public string Header { get; set; }

        public List<string> Namespaces { get; set; }

        public List<CSharpClassResult> Classes { get; set; }

        public string RepositoryNamespace { get; set; }
    }
}