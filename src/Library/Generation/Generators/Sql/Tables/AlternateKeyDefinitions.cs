using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;

namespace Atom.Generation.Generators.Sql.Tables
{
    internal class AlternateKeyDefinitions
    {
        private readonly AtomModel _table;

        public AlternateKeyDefinitions(AtomModel table)
        {
            _table = table;
        }

        public IEnumerable<string> GetDefinitions()
        {
            foreach (var member in _table.Members.Where(i => i.IsAltKey || (i.Unique && !i.Queryable)))
            {
                yield return AlternateKeyConstraints(member);
            }

            yield return CreatePrimaryKeyConstraint(_table.Members.Where(i => i.IsPrimary));

            foreach (var member in _table.Members.Where(i => i.Reference != null)) 
            {
                yield return ForeignKeyConstraint(member);
            }
        }

        private string CreatePrimaryKeyConstraint(IEnumerable<AtomMemberInfo> primaryKeys)
        {
            var data = new
                       {
                           _table.AdditionalInfo.Schema,
                           TableName = _table.Name,
                           MemberNames = string.Join(", ", primaryKeys.Select(i => "[" + i.Name + "]"))
                       };

            return $@"ALTER TABLE [{data.Schema}].[{data.TableName}] ADD CONSTRAINT [PK_{data.TableName}] PRIMARY KEY CLUSTERED ({data.MemberNames}) ON [PRIMARY]
GO";
        }

        private string ForeignKeyConstraint(AtomMemberInfo member)
        {
            var foreignTable = member.Reference.Name;            

            var foreignKeyColumnName = string.IsNullOrWhiteSpace(member.Reference.Member) ? member.Name : member.Reference.Member;

            // fix naming convention if we have multiple columns pointing to the same foreign key
            // the name goes from FK_Table_FkKey to FK_Table_ColumnName

            if (
                member.Atom.Members.Any(
                    i => i.HasReference && i.Reference.Member == member.Reference.Member && i.Reference.Name == member.Reference.Name && i.Name != member.Name))
            {
                foreignKeyColumnName = member.Name;
            }

            string
                Schema = _table.AdditionalInfo.Schema,
                TableName = _table.Name,
                ForeignTable = foreignTable,
                ForeignKeyName = $"FK_{_table.Name}_{foreignTable}_{foreignKeyColumnName}",
                ForeignColumn = member.Reference.Member ?? member.Name,
                MemberName = member.Name,
                schema = member.Atom.AdditionalInfo.Schema;

            string fk = $@"ALTER TABLE [{schema}].[{TableName}] ADD CONSTRAINT [{ForeignKeyName}] FOREIGN KEY ([{MemberName}]) REFERENCES [{Schema}].[{ForeignTable}] ([{ForeignColumn}])
GO";

            return fk;
        }

        private string AlternateKeyConstraints(AtomMemberInfo member)
        {
            string
                Schema = _table.AdditionalInfo.Schema,
                TableName = _table.Name,
                ConstraintName = GetConstraintName(member),
                ConstraintType = GetConstraintType(member),
                MemberName = member.Name,
                schema = member.Atom.AdditionalInfo.Schema;

            string ak = $@"ALTER TABLE [{Schema}].[{TableName}] ADD CONSTRAINT [{ConstraintName}] {ConstraintType} ([{MemberName}]) ON [PRIMARY]
GO"; ;

            return ak;
        }

        private string GetConstraintType(AtomMemberInfo member)
        {
            if (member.IsPrimary)
            {
                return "PRIMARY KEY " + GetConstraintModifier(member);
            }

            if ((member.IsAltKey || member.Unique))
            {
                return "UNIQUE " + GetConstraintModifier(member);
            }

            return string.Empty;
        }

        private string GetConstraintModifier(AtomMemberInfo member)
        {
            if (member.IsPrimary && !(member.MemberType is MemberGuid))
            {
                return "CLUSTERED";
            }

            return "NONCLUSTERED";
        }

        private string GetConstraintName(AtomMemberInfo member)
        {
            if (member.IsPrimary)
            {
                return "PK_" + _table.Name;
            }

            if (member.IsAltKey || member.Unique)
            {
                return $"AK_{_table.Name}_{member.Name}";
            }

            return string.Empty;
        }
    }
}
