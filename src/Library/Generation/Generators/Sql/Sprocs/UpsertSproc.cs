using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class UpsertSproc : BaseStoredProcedureGenerator
    {
        private readonly IReadOnlyList<AtomMemberInfo> _onMembers;

        public UpsertSproc(AtomModel atom, IEnumerable<AtomMemberInfo> onMembers)
            : base(atom)
        {
            _onMembers = onMembers.ToList().AsReadOnly();
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;

            var availableFields = Atom.Members.Where(i => !i.IsPrimary)
                .ToList();

            var inputMembers = availableFields.Where(m => MemberIsUseableAsInsertParameter(m) && !m.HasFlag(MemberFlags.SoftDeleteTracking));

            var fields = availableFields.Select(i => i.Name)
                .ToList();

            var fieldValues = availableFields.Select(GetInsertFields);

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = string.Join(
                    delim,
                    inputMembers.Select(GetTypedSprocParam)
                        .IndentAllLines(1, true)),
                SprocSuffix = "Upsert",
                TableName = Atom.Name,
                Key = GetReturnKey().Name,
                Fields = string.Join(delim, fields.IndentAllLines(4, ignoreFirst: true)),
                Values = string.Join(delim, fieldValues.IndentAllLines(4, ignoreFirst: true)),
                SetList = string.Join(
                    delim,
                    availableFields.Select(GetUpdateValues)
                        .Where(i => i != null)
                        .IndentAllLines(4, true)),
                Temporal = GenerateTemporalNowVariable(Atom.AdditionalInfo.Temporal),
                Lookups = GetLookupSql(availableFields.Where(HasHiddenReference));



            var template = SprocHeader(Schema, TableName, SprocSuffix, Params) +
                           $@"

        {Temporal}
        {Lookups}

    MERGE INTO [{Schema}].[{TableName}] AS [dest]
        USING (VALUES ({string.Join(", ", inputMembers.Select(p => "@" + p.Name))})) 
        AS [source] ({string.Join(", ", inputMembers.Select(p => p.Name))})

        ON ({string.Join(" AND ", _onMembers.Select(m => $"[source].{m.Name} = [dest].{m.Name}"))})

        WHEN MATCHED THEN
            UPDATE
            SET
                {SetList}
        
        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                {Fields}
	        )			
	        VALUES (
                {Values}
	        )

        OUTPUT inserted.{Key};
END
GO
";


            var name = $"{Schema}.{TableName}_{SprocSuffix}";

            return new StoredProcedureResult
            {
                Name = name,
                Sql = template,
                AccessorMetadata = new SqlAccessorMetadata
                {
                    ReturnPrimitive = typeof (int),
                    InputParams = inputMembers,
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.Upsert,
                    Name = name
                }
            };
        }

        private string GetUpdateValues(AtomMemberInfo member)
        {
            if (member.IsPrimary ||
                member.IsAltKey)
            {
                return null;
            }

            if (member.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                if (member.HasFlag(MemberFlags.LastModifiedDateTimetracking))
                {
                    return member.Name + " = @NOW";
                }

                if (member.HasFlag(MemberFlags.CreatedDateTimeTracking))
                {
                    return null;
                }
            }

            if (member.HasFlag(MemberFlags.SoftDeleteTracking))
            {
                return member.Name + " = 0";
            }

            return member.Name + " = @" + member.Name;
        }
    }
}