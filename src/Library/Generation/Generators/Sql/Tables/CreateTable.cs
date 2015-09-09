using System;
using Atom.Data;
using Atom.Generation.Data;

namespace Atom.Generation.Generators.Sql.Tables
{
    internal class CreateTable : ISqlGenerator<CreateTableResult>
    {
        private readonly AtomModel _atomDefinition;

        public CreateTable(AtomModel atomDefinition)
        {
            _atomDefinition = atomDefinition;
        }

        private string Excute()
        {
            string
                Schema = _atomDefinition.AdditionalInfo.Schema,
                TableName = _atomDefinition.Name,
                Members = CreateMembers(_atomDefinition),
                Constraints = CreateConstraints(_atomDefinition),
                Indexes = CreateIndexes(_atomDefinition);

            string CreateTableTemplate = $@"
CREATE TABLE [{Schema}].[{TableName}]
(
{Members}
) ON [PRIMARY]

{Constraints}

{Indexes}";
            return CreateTableTemplate.Trim();
        }

        private string CreateIndexes(AtomModel data)
        {
            return string.Join(Environment.NewLine, new TableIndexDefinitions(data).GetDefinitions());
        }

        private string CreateConstraints(AtomModel data)
        {
            return string.Join(Environment.NewLine, new AlternateKeyDefinitions(data).GetDefinitions());
        }

        private string CreateMembers(AtomModel data)
        {
            return string.Join("," + Environment.NewLine, new AtomMemberDefinitions(data).GetDefinitions());
        }

        public CreateTableResult Generate()
        {
            return new CreateTableResult(_atomDefinition.AdditionalInfo.Schema + "." + _atomDefinition.Name, Excute());
        }
    }
}
