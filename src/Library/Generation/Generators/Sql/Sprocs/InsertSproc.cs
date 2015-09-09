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
    public class InsertSproc : BaseStoredProcedureGenerator
    {
        public InsertSproc(AtomModel atom)
            : base(atom)
        {
        }

        public override StoredProcedureResult Generate()
        {
            var availableFields = Atom.Members.Where(i => !i.IsPrimary && !i.HasFlag(MemberFlags.SoftDeleteTracking))
                .ToList();

            var fields = availableFields.Select(i => i.Name)
                .ToList();

            var fieldValues = availableFields.Select(GetInsertFields);

            var delim = "," + Environment.NewLine;

            var @params = availableFields.Where(MemberIsUseableAsInsertParameter).ToList();

            string
                Schema = Atom.AdditionalInfo.Schema,
                Params = string.Join(delim, @params.Select(GetTypedSprocParam).IndentAllLines(1, true)),
                SprocSuffix = "Insert",
                TableName = Atom.Name,
                Fields = string.Join(delim, fields.IndentAllLines(2, ignoreFirst: true)),
                Values = string.Join(delim, fieldValues.IndentAllLines(2, ignoreFirst: true)),
                ReturnKey = GetReturnKey().Name,
                PrimaryKey = GetPrimaryKey().Name,
                Lookups = GetLookupSql(availableFields.Where(HasHiddenReference)),
                Temporal = GenerateTemporalNowVariable(Atom.AdditionalInfo.Temporal);

            var Select = Atom.AdditionalInfo.SelectAfterInsert.GetValueOrDefault(true)
                ? $@"

	SELECT {ReturnKey} 
	FROM [{Schema}].{TableName}
	WHERE {PrimaryKey} = SCOPE_IDENTITY();
"
                : String.Empty;

            var template = SprocHeader(Schema, TableName, SprocSuffix, Params) +
                           $@"

	{Temporal}
    {Lookups}

    INSERT INTO [{Schema}].{TableName} (
        {Fields}
	)			
	VALUES (
        {Values}
	);
    {Select}
END
GO
";

            var name = $"{Schema}.{TableName}_Insert";

            return new StoredProcedureResult
            {
                Name = name,
                Sql = template,
                AccessorMetadata = new SqlAccessorMetadata
                {
                    ReturnPrimitive = typeof (int),
                    InputParams = @params,
                    BaseAtom = ProjectedAtomRoot.FromAtom(Atom),
                    QueryType = QueryType.Insert,
                    Name = name
                }
            };
        }
    }
}
