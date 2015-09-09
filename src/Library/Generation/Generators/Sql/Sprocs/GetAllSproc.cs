using System;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;
using Atom.Generation.Generators.Sql.Projections;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class GetAllSproc : QuerySproc
    {
        private readonly AtomMemberInfo _byKey;

        public GetAllSproc(AtomModel atom, AtomMemberInfo byKey = null)
            : base(atom)
        {
            _byKey = byKey;
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;
            string Schema = Atom.AdditionalInfo.Schema,
                   Params = string.Empty,
                   SprocSuffix = "GetAll",
                   TableName = Atom.Name;

            var plan = new QueryPlan
            {
                QueryMembers = Atom.Members.Where(IsQueryableColumn).Select(AliasedAtomMemberInfo.FromAtomMemberInfo),
                References = { new SimpleReference(Atom) }
            };

            var template = GenerateQuerySproc(Schema, TableName, SprocSuffix, Params, plan);

            var name = $"{Schema}.{TableName}_GetAll";

            return new StoredProcedureResult
            {
                Name = name,
                Sql = template,
                AccessorMetadata = new SqlAccessorMetadata
                {
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.GetAll,
                    QueryKey = null,
                    Name = name
                }
            };
        }
    }
}
