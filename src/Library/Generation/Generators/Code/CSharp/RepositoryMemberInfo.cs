using Atom.Generation.Data;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class RepositoryMemberInfo
    {
        public string BaseAtomTypeName { get; set; }

        public SqlAccessorMetadata Info { get; set; }
    }
}
