using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Generators.Sql.Sprocs;
using Atom.Generation.Generators.Sql.Tables;

namespace Atom.Generation.Generators.Sql
{
    internal class SqlRoleGenerator : ISqlGenerator<IEnumerable<RoleResult>>
    {
        private readonly DatabaseRole _databaseRole;

        public SqlRoleGenerator(DatabaseRole databaseRole)
        {
            _databaseRole = databaseRole;
        }

        public IEnumerable<RoleResult> Generate()
        {
            string RoleTemplate = $@"
CREATE ROLE [{_databaseRole.Name}]
AUTHORIZATION [dbo]
GO";

            yield return new RoleResult
            {
                Name = _databaseRole.Name,
                Sql = RoleTemplate
            };
        }
    }
}
