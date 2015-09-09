using System.Collections.Generic;
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
    public class SearchBySproc : BaseStoredProcedureGenerator
    {
        private readonly AtomReference _searchBy;

        private readonly IEnumerable<AtomModel> _allAtoms;

        public SearchBySproc(AtomModel atomModel, AtomReference searchBy, IEnumerable<AtomModel> allAtoms)
            : base(atomModel)
        {
            _searchBy = searchBy;
            _allAtoms = allAtoms;
        }

        public override StoredProcedureResult Generate()
        {
            var fields = new Dictionary<string, List<string>>
                         {
                             {
                                 Atom.Name, Atom.Members.Select(i => i.Name)
                                                .ToList()
                             },
                             {
                                 _searchBy.Name, _searchBy.TargetMember.Atom.Members.Select(i => i.Name)
                                                          .ToList()
                             }
                         };

            var projectionAtom = new ProjectionAtom(fields);

            var builder = new QueryPlanBuilder(projectionAtom, _allAtoms.ToDictionary(i => i.Name));

            var plan = builder.Build();

            plan.QueryMembers = plan.QueryMembers.Where(i => i.Member.Atom.Name != _searchBy.Name && !i.Member.HasFlag(MemberFlags.Generated))
                                    .ToList();

            var queryPlan = new QuerySqlGenerator(plan, 2).Generate()
                                                          .IndentAllLines(2, true);

            var sprocSuffix = $"ListBy{_searchBy.Member}";

            string
                Params = GetTypedSprocParam(_searchBy.TargetMember),
                schema = Atom.AdditionalInfo.Schema,
                Key = _searchBy.Member,
                ExternalTable = _searchBy.Name,
                ExternalKey = _searchBy.Member,
                TableName = Atom.Name,
                SprocSuffix = sprocSuffix;

            var template = SprocHeader(schema, TableName, SprocSuffix, Params) + $@"

        {queryPlan}
        WHERE
            [{schema}].[{ExternalTable}].{Key} = @{Key}	
END
GO
";

            var name = $"{schema}.{TableName}_ListBy{ExternalKey}";

            var result = new StoredProcedureResult
                         {
                             Name = name,
                             Sql = template,
                             AccessorMetadata = new SqlAccessorMetadata
                                    {
                                        BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                                        QueryKey = _searchBy.TargetMember,
                                        QueryType = QueryType.GetOne,
                                        Name = name
                                    }
                         };

            return result;
        }
    }
}
