using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;

namespace Atom.Generation.Generators.Sql
{
    internal class SqlSchemaGenerator : ISqlGenerator<SchemaResult>
    {
        private readonly string _schema;
        private readonly List<DatabaseRole> _roles;

        public SqlSchemaGenerator(string schema, List<DatabaseRole> roles)
        {
            _schema = schema;
            _roles = roles;
        }

        public SchemaResult Generate()
        {
            var template = $@"{GenerateSchemaDefinition()}{GenerateGrants()}";
            return new SchemaResult
            {
                Sql = template
            };
        }

        private string GenerateSchemaDefinition()
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(Constants.DefaultSchema, _schema))
            {
                return string.Empty;
            }

            return $@"
CREATE SCHEMA [{_schema}]
AUTHORIZATION [dbo]
GO
";
        }

        public string GenerateGrants()
        {
            return string.Join(Environment.NewLine, _roles.Select(GenerateGrant));
        }

        private string GenerateGrant(DatabaseRole role)
        {
            return $@"
GRANT SELECT  ON SCHEMA:: [{_schema}] TO [{role.Name}]
GRANT INSERT  ON SCHEMA:: [{_schema}] TO [{role.Name}]
GRANT DELETE  ON SCHEMA:: [{_schema}] TO [{role.Name}]
GRANT UPDATE  ON SCHEMA:: [{_schema}] TO [{role.Name}]
GRANT EXECUTE ON SCHEMA:: [{_schema}] TO [{role.Name}]

GO
";
        }
    }
}
