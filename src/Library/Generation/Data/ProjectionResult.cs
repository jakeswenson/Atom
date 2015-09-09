using System.Collections.Generic;

namespace Atom.Generation.Data
{
    public class ProjectionResult
    {
        public string Name { get; set; }

        public string Sql { get; set; }

        public SqlAccessorMetadata Meta { get; set; }

        public List<Reference> References { get; set; }
    }
}
