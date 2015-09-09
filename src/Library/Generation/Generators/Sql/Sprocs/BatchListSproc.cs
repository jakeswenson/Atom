using System;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;
using Atom.Generation.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Generators.Sql.Sprocs
{
    public class BatchListSproc : BaseStoredProcedureGenerator
    {
        private readonly AtomMemberInfo _byKey;

        public BatchListSproc(AtomModel atom, AtomMemberInfo byKey = null)
            : base(atom)
        {
            _byKey = byKey;
        }

        public override StoredProcedureResult Generate()
        {
            var queryKey = _byKey ?? GetReturnKey();

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = "@LookupKeys [" + GetTableType(queryKey) + "] READONLY",
                LookupColumnName = GetLookupColumnName(queryKey),
                SprocSuffix = "GetMany",
                Description = "Batch Gets " + Atom.Name + " by " + queryKey.Name,
                TableName = Atom.Name,
                Key = queryKey.Name,
                Fields = string.Join("," + Environment.NewLine, Atom.Members.Where(IsListableField).Select(a => a.Name).IndentAllLines(3)),
                Hint = Atom.AdditionalInfo.UseWithNoLockHint.GetValueOrDefault(true) ? " with (NOLOCK)" : String.Empty;

            var template = SprocHeader(Schema, TableName, SprocSuffix, Params) + $@"
        

        SELECT 
{Fields}
        FROM
            [{Schema}].{TableName}{Hint}
        INNER JOIN  @LookupKeys lk ON lk.{LookupColumnName} = [{Schema}].{TableName}.{Key}        
END
GO
";
            

            var name = $"{Schema}.{TableName}_GetMany";

            return new StoredProcedureResult
                   {
                       Name = name,
                       Sql = template,
                       AccessorMetadata = new SqlAccessorMetadata
                              {
                                  BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                                  QueryType = QueryType.BatchList,
                                  QueryKey = queryKey,
                                  Name = name
                              }
                   };
        }

        private string GetLookupColumnName(AtomMemberInfo key)
        {
            return "id";
        }

        private string GetTableType(AtomMemberInfo key)
        {
            if (key.MemberType is MemberGuid)
            {
                return Constants.GuidTableType;
            }

            if (key.MemberType is MemberLong)
            {
                return Constants.BigIntTableType;
            }

            throw new Exception($"Custom table type didn't exist for column {key.Name} with type {key.Type}");
        }

        private bool IsListableField(AtomMemberInfo arg)
        {
            if (arg.IsPrimary && Atom.AdditionalInfo.ShouldHidePrimaryKey())
            {
                return false;
            }

            if (IsKey(arg))
            {
                return true;
            }

            if (IsHiddenTemporal(arg))
            {
                return false;
            }

            return true;
        }
    }
}
