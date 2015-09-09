using System;
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
    public class GetBySproc : QuerySproc
    {
        private readonly AtomMemberInfo _byKey;
        private readonly bool _isUnique;

        public GetBySproc(AtomModel atom, AtomMemberInfo byKey, bool isUnique)
            : base(atom)
        {
            _byKey = byKey;
            _isUnique = isUnique;
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;
            
            var queryKey = _byKey;

            if (_byKey.HasReference && _byKey.Reference.IsReferenceToHiddenPrimaryKey)
            {
                queryKey = queryKey.Reference.TargetAtomAlternateKey;
            }

            string sprocSuffix = _isUnique ? $"GetOneBy{queryKey.Name}" : $"GetBy{queryKey.Name}" ;

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = GetTypedSprocParam(queryKey),
                TableName = Atom.Name,
                Key = queryKey.Name;

            var plan = new QueryPlan
            {
                QueryMembers = Atom.Members.Where(IsQueryableColumn).Select(AliasedAtomMemberInfo.FromAtomMemberInfo),
                References = { new SimpleReference(Atom) },
                Filters = { new AtomFilter { FilterValue = "@" + Key, Member = queryKey } }
            };

            if (queryKey != _byKey)
            {
                var simpleRef = new SimpleReference(queryKey.Atom);
                plan.References = new List<Reference>
                {
                    simpleRef,
                    new ResolvedReference(Atom, new List<Reference>
                    {
                        simpleRef
                    })
                };
            }

            var template = GenerateQuerySproc(Schema, TableName, sprocSuffix, Params, plan);

            var name = $"{Schema}.{TableName}_{sprocSuffix}";

            return new StoredProcedureResult
                   {
                       Name = name,
                       Sql = template,
                       AccessorMetadata = new SqlAccessorMetadata
                              {
                                  BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                                  QueryType = _isUnique ? QueryType.GetOne : QueryType.GetBy,
                                  QueryKey = queryKey,
                                  Name = name
                              }
                   };
        }
    }
}
