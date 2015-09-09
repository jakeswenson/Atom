using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Data.Projections;
using Atom.Generation;

namespace Atom.Generation.Generators.Sql.Projections
{
    public class QuerySqlGenerator : ISqlGenerator<string>
    {
        private readonly QueryPlan _queryPlan;

        private readonly int _indentLevel;

        public QuerySqlGenerator([NotNull] QueryPlan queryPlan, int indentLevel = 0)
        {
            if (queryPlan == null)
            {
                throw new ArgumentNullException("queryPlan");
            }

            _queryPlan = queryPlan;
            _indentLevel = indentLevel;
        }

        public string Generate()
        {
            var projectionColumns = GetProjectionColumns(_queryPlan);
            var projectionColumnsSql = string.Join("," + Environment.NewLine, projectionColumns).IndentAllLines(2, ignoreFirst: true);

            var simpleTables = _queryPlan.References.OfType<SimpleReference>()
                .Select(r => r.Name + (r.ReferenceTarget.AdditionalInfo.UseWithNoLockHint.GetValueOrDefault(true) ? " WITH(NOLOCK)" : String.Empty));
            string selectIndent = Environment.NewLine.AppendTab(_indentLevel);

            string template = 
                string.Join(
                    selectIndent,  
                    "SELECT",
                    $"    {projectionColumnsSql}",
                    $"FROM {string.Join(", ", simpleTables)}" ,
                    string.Join(selectIndent, GetQueryJoinsAndWhere()));

            return template;
        }

        private IEnumerable<string> GetQueryJoinsAndWhere()
        {
            string selectIndent = Environment.NewLine.AppendTab(_indentLevel);

            var joins = GetJoins(_queryPlan.References.OfType<ResolvedReference>());

            if (joins.Any())
            {
                foreach (var innerJoin in joins)
                {
                    yield return innerJoin;
                }
            }

            var filter = GetProjectionFilter(_queryPlan);
            yield return filter;
        }

        private string GetProjectionFilter(QueryPlan queryPlan)
        {
            if (queryPlan.Filters.Any())
            {
                var result = "WHERE" + Environment.NewLine.AppendTab(_indentLevel + 1);
                return result +
                       string.Join(
                           (" AND " + Environment.NewLine).AppendTab(_indentLevel + 1),
                           queryPlan.Filters
                                    .Select(
                                        m => $"([{m.Member.Atom.Name}].{m.Member.Name} = {m.FilterValue})"));
            }

            return string.Empty;
        }

        private IEnumerable<string> GetJoins(IEnumerable<ResolvedReference> joinReferences)
        {
            var hiddenPrimaryKeyReferences = _queryPlan.QueryMembers.Where(m => IsReferenceToHiddenPrimaryKey(m)).Select(m => m.Member);
            var membersNeedingJoins =
                hiddenPrimaryKeyReferences.Where(m => !_queryPlan.References.Any(r => r.Name == m.Reference.TargetMember.Atom.Name));
            var references = joinReferences.ToList();
            references.AddRange(
                membersNeedingJoins.Select(member => new ResolvedReference(member.Reference.TargetMember.Atom,
                    new List<Reference>
                    {
                        new SimpleReference(member.Atom)
                    })));

            foreach (var joinReference in references)
            {
                var onConditions = GetOnConditions(joinReference).ToList();

                string prefix = string.Empty;

                if (onConditions.Count > 1)
                {
                    prefix = Environment.NewLine.AppendTab(_indentLevel + 2);
                }

                var onConditionsSql = prefix + string.Join(" AND " + Environment.NewLine.AppendTab(_indentLevel + 2), onConditions);
                var hint = joinReference.ReferenceTarget.AdditionalInfo.UseWithNoLockHint.GetValueOrDefault(true) ? " WITH (NOLOCK)" : String.Empty;
                var needsLeft = joinReference.ReferencingMembers.Any(r => r.ThroughMember.Optional);
                var joinType = needsLeft ? "LEFT" : "INNER";
                yield return $"{joinType} JOIN [dbo].[{joinReference.Name}]{hint} ON {onConditionsSql}";
            }
        }

        private IEnumerable<string> GetOnConditions(ResolvedReference joinReference)
        {
            return joinReference.ReferencingMembers.Select(referenceMember =>
            {
                string refName = referenceMember.Target.Name;
                string refColumn = referenceMember.ThroughMember.Name;

                return $"[{refName}].[{refColumn}] = [{joinReference.Name}].[{refColumn}]";
            });
        }

        private IEnumerable<string> GetProjectionColumns(QueryPlan queryPlan)
        {
            return queryPlan.QueryMembers.Select(GetProjectionColumn);
        }

        private string GetProjectionColumn(AliasedAtomMemberInfo prop)
        {
            string projectionColumn = $"[{prop.Member.Atom.Name}].{prop.Member.Name}";

            if (IsReferenceToHiddenPrimaryKey(prop))
            {
                var member = prop.Member.Reference.TargetAtomAlternateKey;
                projectionColumn = $"[{member.Atom.Name}].{member.Name}";
            }

            if (prop.HasAlias)
            {
                return projectionColumn + $"AS {prop.Name}";
            }

            return projectionColumn;
        }

        private bool IsReferenceToHiddenPrimaryKey(AliasedAtomMemberInfo prop)
        {
            return prop.Member.HasReference && prop.Member.Reference.IsReferenceToHiddenPrimaryKey;
        }
    }
}
