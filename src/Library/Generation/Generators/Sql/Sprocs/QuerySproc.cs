using System;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Generators.Sql.Projections;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public abstract class QuerySproc : BaseStoredProcedureGenerator
    {
        protected QuerySproc(AtomModel atom)
            : base(atom)
        {
        }

        public string GenerateQuerySproc(
            string schemaName,
            string tableName, 
            string sprocSuffix, 
            string sprocParameters,
            QueryPlan plan)
        {
            AddDeletedItemFiltering(plan);

            QuerySqlGenerator query = new QuerySqlGenerator(plan, indentLevel: 1);
            return SprocHeader(schemaName, tableName, sprocSuffix, sprocParameters) + $@"

    {query.Generate()}

END
GO
";
        }

        private void AddDeletedItemFiltering(QueryPlan plan)
        {
            var deletionMember = Atom.Members.FirstOrDefault(m => m.HasFlag(MemberFlags.SoftDeleteTracking));

            if (Atom.AdditionalInfo.FilterDeleted.GetValueOrDefault() && deletionMember != null)
            {
                plan.Filters.Add(
                    new AtomFilter
                    {
                        FilterValue = "0",
                        Member = deletionMember
                    });
            }
        }

        protected bool IsQueryableColumn(AtomMemberInfo arg)
        {
            if (arg.IsPrimary &&
                Atom.AdditionalInfo.ShouldHidePrimaryKey())
            {
                return false;
            }

            if (IsKey(arg))
            {
                return true;
            }

            return true;
        }
    }
}