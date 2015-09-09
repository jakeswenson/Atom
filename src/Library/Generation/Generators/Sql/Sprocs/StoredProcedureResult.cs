using Atom.Generation.Data;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class StoredProcedureResult
    {
        public string Name { get; set; }

        public string Sql { get; set; }

        public SqlAccessorMetadata AccessorMetadata { get; set; }
    }
}
