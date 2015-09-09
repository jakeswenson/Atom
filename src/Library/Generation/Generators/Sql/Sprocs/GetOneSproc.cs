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
    public class GetOneSproc : QuerySproc
    {
        public GetOneSproc(AtomModel atom)
            : base(atom)
        {
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;
            var queryKey = GetReturnKey();

            string Schema = Atom.AdditionalInfo.Schema,
                   Params = GetTypedSprocParam(queryKey),
                   SprocSuffix = "GetOne",
                   TableName = Atom.Name,
                   Key = queryKey.Name;

            var plan = new QueryPlan
            {
                QueryMembers = Atom.Members.Where(IsQueryableColumn).Select(AliasedAtomMemberInfo.FromAtomMemberInfo),
                References = { new SimpleReference(Atom) },
                Filters = { new AtomFilter { FilterValue = "@" + Key, Member = queryKey } }
            };

            var template = GenerateQuerySproc(Schema, TableName, SprocSuffix, Params, plan);

            var name = $"{Schema}.{TableName}_GetOne";

            return new StoredProcedureResult
            {
                Name = name,
                Sql = template,
                AccessorMetadata = new SqlAccessorMetadata
                {
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.GetOne,
                    QueryKey = queryKey,
                    Name = name
                }
            };
        }
    }
}