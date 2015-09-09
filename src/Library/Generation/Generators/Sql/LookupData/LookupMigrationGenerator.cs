using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Generation.Extensions;
using Atom.Generation.Generators.Sql.Types;

namespace Atom.Generation.Generators.Sql.LookupData
{
    public abstract class BaseLookupGenerator
    {
        public BaseLookupGenerator(AtomModel lookup) : this(new LookupContext(lookup))
        {
        }

        public BaseLookupGenerator(LookupContext lookupContext)
        {
            LookupContext = lookupContext;
        }

        protected LookupContext LookupContext { get; }

        protected IEnumerable<string> GetInsertColumns()
        {
            yield return $"[{LookupContext.IdMember.Name}]";
            yield return $"[{LookupContext.NameMember.Name}]";
            yield return $"[{LookupContext.DescriptionMember.Name}]";

            foreach (var column in LookupContext.OtherMembers)
            {
                yield return $"[{column.Name}]";
            }

            if (LookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                yield return $"[{LookupContext.CreatedOnMember.Name}]";
                yield return $"[{LookupContext.LastModifiedMember.Name}]";
            }

            if (LookupContext.Atom.AdditionalInfo.UseSoftDeletes.GetValueOrDefault())
            {
                yield return $"[{LookupContext.SoftDeleteMember.Name}]";
            }
        }
    }

    public class LookupDataGenerator : BaseLookupGenerator
    {
        public LookupDataGenerator(AtomModel lookup) : base(lookup)
        {
        }

        public LookupDataGenerator(LookupContext lookupContext) : base(lookupContext)
        {
        }

        public string GetDataMigrationSqlForLookup()
        {
            string insertValues = string.Join("," + Environment.NewLine, LookupContext.Atom.Lookup.Values.Select(GetInsertValues));

            List<string> inserts = 
                LookupContext.Atom.Lookup.Values
                    .Select(GetInsertValues)
                    .Select(values => $"INSERT INTO [{LookupContext.Schema}].[{LookupContext.TableName}] ({string.Join(", ", GetInsertColumns())}) VALUES ({values});")
                    .ToList();

            var template = string.Join(Environment.NewLine, inserts);

            return template;
        }

        private string GetInsertValues(LookupValue arg, int idx)
        {
            string Id = (arg.Index ?? (idx + 1)).ToString(),
                Name = $"'{arg.Name}'",
                Description = $"N'{arg.Description}'";

            List<string> insertValues = new List<string>
            {
                Id,
                Name,
                Description
            };

            insertValues.AddRange(
                arg.OtherColumns.Select(Convert.ToBoolean)
                   .Select(v => v ? "1" : "0"));

            if (LookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                insertValues.Add($"'{DateTime.UtcNow.ToString()}'");
                insertValues.Add($"'{DateTime.UtcNow.ToString()}'");
            }

            if (LookupContext.Atom.AdditionalInfo.UseSoftDeletes.GetValueOrDefault())
            {
                insertValues.Add(arg.IsDeleted ? "1" : "0");
            }

            return $@"{string.Join(", ", insertValues)}";
        }
    }

    public class LookupMigrationGenerator
    {
        private readonly LookupContext lookupContext;

        public LookupMigrationGenerator(AtomModel lookup) : this(new LookupContext(lookup))
        {
        }

        public LookupMigrationGenerator(LookupContext lookupContext)
        {
            this.lookupContext = lookupContext;
        }


        public string GetDataMigrationSqlForLookup()
        {
            string insertValues = string.Join("," + Environment.NewLine, lookupContext.Atom.Lookup.Values.Select(GetInsertValues));
            string variableTableName = lookupContext.TableName + "Data";

            string variableTableColumns = string.Join("," + Environment.NewLine, GetVariableTableColumnDefs().Select(line => line.WithTab(1)));

            var template =
                $@"--
-- Data for lookup table '{lookupContext.Atom.Name}'
--
DECLARE @{variableTableName} TABLE (
{variableTableColumns}
);

INSERT INTO @{variableTableName}({string.Join(", ", GetVariableTableInsertColumnNames())})
VALUES
{insertValues};

DECLARE @NOW DATETIME = GETUTCDATE();

MERGE INTO [{lookupContext.Schema}].[{lookupContext.TableName}] AS [dest]
USING @{variableTableName} AS [source]
ON([dest].{lookupContext.IdMember.Name} = [source].{lookupContext.IdMember.Name})

WHEN MATCHED THEN
    UPDATE SET
{string.Join("," + Environment.NewLine, GetUpdateColumns().Select(line => line.WithTab(2)))}

WHEN NOT MATCHED BY TARGET THEN
    INSERT ({string.Join(", ", GetInsertColumns())})
    VALUES ({string.Join(", ", GetInsertValues())})

{GenerateDeletes()}

OUTPUT 
	$action, 
	[INSERTED].{lookupContext.IdMember.Name},
	[DELETED].{lookupContext.NameMember.Name} AS Old{lookupContext.NameMember.Name}, 
	[INSERTED].{lookupContext.NameMember.Name};
";
            return template;
        }

        private IEnumerable<string> GetVariableTableColumnDefs()
        {
            SqlTypeNameVistor typeVisitor = new SqlTypeNameVistor();

            yield return $"{lookupContext.IdMember.Name} {lookupContext.IdMember.MemberType.Accept(typeVisitor)} NOT NULL";
            yield return $"{lookupContext.NameMember.Name} {lookupContext.NameMember.MemberType.Accept(typeVisitor)} NOT NULL";
            yield return $"{lookupContext.DescriptionMember.Name} {lookupContext.DescriptionMember.MemberType.Accept(typeVisitor)} NOT NULL";

            foreach (var column in lookupContext.OtherMembers)
            {
                yield return $"{column.Name} {column.MemberType.Accept(typeVisitor)} NOT NULL";
            }
        }

        private IEnumerable<string> GetVariableTableInsertColumnNames()
        {
            yield return $"[{lookupContext.IdMember.Name}]";
            yield return $"[{lookupContext.NameMember.Name}]";
            yield return $"[{lookupContext.DescriptionMember.Name}]";

            foreach (var col in lookupContext.OtherMembers)
            {
                yield return $"[{col.Name}]";
            }
        }

        private IEnumerable<string> GetUpdateColumns()
        {
            yield return $"{lookupContext.NameMember.Name} = [source].{lookupContext.NameMember.Name}";
            yield return $"{lookupContext.DescriptionMember.Name} = [source].{lookupContext.DescriptionMember.Name}";

            foreach (var mem in lookupContext.OtherMembers)
            {
                yield return $"{mem.Name} = [source].{mem.Name}";
            }

            if (lookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                yield return $"{lookupContext.LastModifiedMember.Name} = @NOW";
            }
        }

        private IEnumerable<string> GetInsertColumns()
        {
            yield return $"[{lookupContext.IdMember.Name}]";
            yield return $"[{lookupContext.NameMember.Name}]";
            yield return $"[{lookupContext.DescriptionMember.Name}]";

            foreach (var column in lookupContext.OtherMembers)
            {
                yield return $"[{column.Name}]";
            }

            if (lookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                yield return $"[{lookupContext.CreatedOnMember.Name}]";
                yield return $"[{lookupContext.LastModifiedMember.Name}]";
            }
        }

        private IEnumerable<string> GetInsertValues()
        {
            yield return $"[source].[{lookupContext.IdMember.Name}]";
            yield return $"[source].[{lookupContext.NameMember.Name}]";
            yield return $"[source].[{lookupContext.DescriptionMember.Name}]";

            foreach (var mem in lookupContext.OtherMembers)
            {
                yield return $"[source].[{mem.Name}]";
            }

            if (lookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                yield return $"@NOW";
                yield return $"@NOW";
            }
        }

        private string GetInsertValues(LookupValue arg, int idx)
        {
            string Id = (arg.Index ?? (idx + 1)).ToString(),
                   Name = $"'{arg.Name}'",
                   Description = $"N'{arg.Description}'";

            List<string> insertValues = new List<string>
            {
                Id,
                Name,
                Description
            };

            insertValues.AddRange(
                arg.OtherColumns.Select(Convert.ToBoolean)
                   .Select(v => v ? "1" : "0"));

            return $@"({string.Join(", ", insertValues)})";
        }

        private string GenerateDeletes()
        {
            var deleteMember = lookupContext.Atom.Members.FirstOrDefault(m => m.HasFlag(MemberFlags.SoftDeleteTracking));

            if (deleteMember == null)
            {
                return string.Empty;
            }

            string lastModifiedUpdate = string.Empty;

            if (lookupContext.Atom.AdditionalInfo.Temporal.HasTemporal.GetValueOrDefault())
            {
                lastModifiedUpdate = $",{lookupContext.LastModifiedMember.Name} = @NOW";
            }

            return $@"
WHEN NOT MATCHED BY SOURCE THEN
    UPDATE SET
        {deleteMember.Name} = 1
        {lastModifiedUpdate}".Trim();

        }
    }
}