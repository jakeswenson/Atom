using System;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class DeleteSproc : BaseStoredProcedureGenerator
    {
        public DeleteSproc(AtomModel atom)
            : base(atom)
        {
        }

        public override StoredProcedureResult Generate()
        {
            var delim = "," + Environment.NewLine;

            var inputParams = Atom.Members.Where(i => IsKey(i)).ToList();
            var queryKey = GetReturnKey();

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = string.Join(
                    delim,
                    inputParams.Select(GetTypedSprocParam)
                               .IndentAllLines(1, true)),
                SprocSuffix = "Delete",
                TableName = Atom.Name,
                Key = GetReturnKey()
                    .Name,
                SetList = string.Join(
                    delim,
                    Atom.Members.Select(GetUpdateValues)
                        .Where(i => i != null)
                        .IndentAllLines(3, true)),
                Temporal = GenerateTemporalNowVariable(Atom.AdditionalInfo.Temporal);

            var template = SprocHeader(Schema, TableName, SprocSuffix, Params) + $@"

        {Temporal}
        
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
                    InputParams = inputParams,
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.Delete,
                    Name = name,
                    QueryKey = queryKey
                }
            };
        }

        private string GetUpdateValues(AtomMemberInfo member)
        {
            if (member.HasFlag(MemberFlags.SoftDeleteTracking))
            {
                return member.Name + " = 1";
            }

            if (member.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault() &&
                member.HasFlag(MemberFlags.LastModifiedDateTimetracking))
            {
                return member.Name + " = @NOW";
            }

            return null;
        }
    }
}