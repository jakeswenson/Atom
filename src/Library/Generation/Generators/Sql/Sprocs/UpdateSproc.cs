using System;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class UpdateSproc : BaseStoredProcedureGenerator
    {
        public UpdateSproc(AtomModel atom)
            : base(atom)
        {
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;

            var inputMembers = Atom.Members.Where(i => !i.HasFlag(MemberFlags.NotUpdateable) || IsKey(i));

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = string.Join(
                    delim,
                    inputMembers.Select(GetTypedSprocParam)
                               .IndentAllLines(1, true)),
                SprocSuffix = "Update",
                TableName = Atom.Name,
                Key = GetReturnKey()
                    .Name,
                SetList = string.Join(
                    delim,
                    Atom.Members.Select(GetUpdateValues)
                        .Where(i => i != null)
                        .IndentAllLines(3, true)),
                Temporal = GenerateTemporalNowVariable(Atom.AdditionalInfo.Temporal),
                Lookups = GetLookupSql(inputMembers.Where(HasHiddenReference));

            var template = SprocHeader(Schema, TableName, SprocSuffix, Params) + $@"

        {Temporal}
        {Lookups}
        
        UPDATE [{Schema}].{TableName}
        SET 
            {SetList}
        
        WHERE
            {Key} = @{Key}	
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
                    ReturnPrimitive = typeof(int),
                    InputParams = inputMembers,
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.Update,
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

            return member.Name + " = @" + member.Name;
        }
    }
}
