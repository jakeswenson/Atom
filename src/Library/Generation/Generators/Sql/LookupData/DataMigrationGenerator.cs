using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Generators.Sql.Tables;

namespace Atom.Generation.Generators.Sql.LookupData
{
    public class DataMigrationGenerator : ISqlGenerator<CreateTableResult>
    {
        private readonly IEnumerable<AtomModel> _lookups;

        public DataMigrationGenerator(IEnumerable<AtomModel> atoms)
        {
            _lookups = atoms.Where(t => t.IsLookup && t.AdditionalInfo.ShouldGenerateCode());
        }

        
        public CreateTableResult Generate()
        {
            var sql = _lookups.Select(l => new LookupMigrationGenerator(l)).Select(gen => gen.GetDataMigrationSqlForLookup());

            var template = $@"
-- Last Generated: {DateTime.Now.ToShortDateString()}
{string.Join(Environment.NewLine + "GO" + Environment.NewLine, sql)}
GO
";

            return new CreateTableResult($"AfterSchemaDeploy_MigrateData", template);
        }
    }
}