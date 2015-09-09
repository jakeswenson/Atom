using System.Collections.Generic;
using Atom.Data;

namespace Atom.Generation.Generators.Sql.Tables
{
    public class TableTypeGenerator
    {
        public IEnumerable<TableType> GetCustomTableTypes()
        {
            yield return GuidTableTypeGen();

            yield return BigIntTableTypeGen();
        }

        private TableType BigIntTableTypeGen()
        {
            return new TableType
            {
                Name = Constants.BigIntTableType,
                Sql = $@"
CREATE TYPE [dbo].[{Constants.BigIntTableType}] AS TABLE
(
[id] [bigint] NOT NULL
)
GO
"
            };
        }

        private TableType GuidTableTypeGen()
        {
            return new TableType
            {
                Name = Constants.GuidTableType,
                Sql = $@"
CREATE TYPE [dbo].[{Constants.GuidTableType}] AS TABLE
(
[id] [uniqueidentifier] NOT NULL
)
GO
"
            };
        }
    }
}
