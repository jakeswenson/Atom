using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Data.Serialization;
using Atom.Generation.Data;
using Atom.Generation.Generators.Sql.LookupData;
using Atom.Generation.Generators.Sql.Sprocs;
using Atom.Generation.Generators.Sql.Tables;
using Atom.Generation.Generators.Sql.Projections;
using Serilog;

namespace Atom.Generation.Generators.Sql
{
    public class SqlGenerator : IGenerator<SqlResult, SqlTargetConfig>
    {
        public static CreateTableResult CreateTable(AtomModel atom)
        {
            return new CreateTable(atom).Generate();
        }

        public static IEnumerable<StoredProcedureResult> MakeSprocs(AtomModel atomModel, IReadOnlyList<AtomModel> atoms)
        {
            if (ShouldCreate(atomModel, QueryFlags.Insert))
            {
                yield return new InsertSproc(atomModel).Generate();
            }

            if (ShouldCreate(atomModel, QueryFlags.Update))
            {
                yield return new UpdateSproc(atomModel).Generate();
            }

            if (ShouldCreate(atomModel, QueryFlags.GetOne))
            {
                yield return new GetOneSproc(atomModel).Generate();
            }

            if (ShouldCreate(atomModel, QueryFlags.GetAll))
            {
                yield return new GetAllSproc(atomModel).Generate();
            }

            if (ShouldCreate(atomModel, QueryFlags.BatchGet))
            {
                yield return new BatchListSproc(atomModel).Generate();
            }

            if (ShouldCreate(atomModel, QueryFlags.GetBy))
            {
                foreach (var keyAccessor in atomModel.Members.Where(i => IsAccessReference(i) || IsNonHiddenQueryable(i)))
                {
                    yield return new GetBySproc(atomModel, keyAccessor, keyAccessor.Unique).Generate();
                }

                foreach (var indexDef in atomModel.IndexOn.Values.Where(i => i.Columns.Count == 1))
                {
                    yield return new GetBySproc(atomModel, indexDef.GetIndexMembers(atomModel).First(), indexDef.Unique).Generate();
                }
            }

            if (ShouldCreate(atomModel, QueryFlags.Delete))
            {
                if (!atomModel.AdditionalInfo.UseSoftDeletes.GetValueOrDefault())
                {
                    throw new Exception($"Delete sproc requested for model '{atomModel.Name}' but soft delete is not enabled for the model. Try setting useSoftDeletes.");
                }

                yield return new DeleteSproc(atomModel).Generate();
            }

            if (atomModel.UpsertOn?.Count > 0)
            {
                yield return new UpsertSproc(atomModel, atomModel.SelectMembers(atomModel.UpsertOn)).Generate();
            }

            if (atomModel.SearchableBy?.Count > 0)
            {
                foreach (var searchBy in atomModel.SearchableBy)
                {
                    yield return new SearchBySproc(atomModel, searchBy, atoms).Generate();
                }
            }
        }

        private static bool IsNonHiddenQueryable(AtomMemberInfo i)
        {
            return i.Queryable && !i.HasFlag(MemberFlags.Hidden) && !i.IsPrimary && !i.IsAltKey;
        }

        private static bool IsAccessReference(AtomMemberInfo i)
        {
            return i.HasReference && i.Reference.Access;
        }

        public static ProjectionResult MakeView(ProjectionAtom projection, IEnumerable<AtomModel> allAtoms)
        {
            return new ViewGenerator(projection, allAtoms).Generate();
        }

        public static ProjectionResult MakeQuery(ProjectionAtom projection, IEnumerable<AtomModel> allAtoms)
        {
            return new CustomQueryStoredProcedureGenerator(projection, allAtoms).Generate();
        }

        public static IEnumerable<TableType> MakeCustomTables()
        {
            return new TableTypeGenerator().GetCustomTableTypes();
        }

        public SqlResult Generate(GeneratorArguments<SqlTargetConfig> generatorArguments)
        {
            var sqlResult = new SqlResult
            {
                SqlAccessors = new List<SqlAccessorMetadata>()
            };

            var atoms = generatorArguments.Atoms;

            List<CreateTableResult> tables = new List<CreateTableResult>();
            List<StoredProcedureResult> sprocs = new List<StoredProcedureResult>();

            foreach (var atom in atoms.Where(a => a.AdditionalInfo.ShouldGenerateCode()))
            {
                var table = AddTable(sqlResult, atom);

                tables.Add(table);

                Log.Information("Creating default sprocs: {AtomName}", atom.Name);

                foreach (var sproc in MakeSprocs(atom, atoms))
                {
                    sqlResult.SqlAccessors.Add(sproc.AccessorMetadata);

                    sqlResult.AddOutput("Stored Procedures", sproc.Name, sproc.Sql);

                    sprocs.Add(sproc);
                }
            }

            var dataMigration = new DataMigrationGenerator(atoms).Generate();
            sqlResult.AddOutput("../DataRepair/AlwaysRun", dataMigration.Name, dataMigration.TableSql);

            foreach (var lookup in atoms.Where(a => a.IsLookup && a.AdditionalInfo.GenerateLookupData.GetValueOrDefault()))
            {
                sqlResult.AddLookupOutput($"{lookup.AdditionalInfo.Schema}.{lookup.Name}_Data", new LookupDataGenerator(lookup).GetDataMigrationSqlForLookup());
            }

            AddCustomTableTypes(sqlResult);

            AddSchemas(sqlResult, atoms, generatorArguments.Defaults.Roles);

            AddRoles(sqlResult, generatorArguments.Defaults.Roles);

            foreach (var projectionAtom in generatorArguments.Projections.OfType<ViewProjectionAtom>())
            {
                Log.Information("Making view for {ViewName}", projectionAtom.Name);

                var result = MakeView(projectionAtom, atoms.Where(i => i.AdditionalInfo.Schema == Constants.DefaultSchema));

                sqlResult.SqlAccessors.Add(result.Meta);

                sqlResult.AddOutput("Views", result.Name, result.Sql);
            }

            foreach (var projectionAtom in generatorArguments.Projections.OfType<ViewProjectionAtom>())
            {
                Log.Information("Making queries for {ViewName}", projectionAtom.Name);

                var result = MakeView(projectionAtom, atoms.Where(i => i.AdditionalInfo.Schema == Constants.DefaultSchema));

                sqlResult.SqlAccessors.Add(result.Meta);

                sqlResult.AddQueryOutput(result.Name, result.Sql);
            }

            return sqlResult;
        }

        private void AddRoles(SqlResult sqlResult, List<DatabaseRole> roles)
        {
            foreach (var databaseRole in roles)
            {
                var result = new SqlRoleGenerator(databaseRole).Generate();
                foreach (var roleResult in result)
                {
                    sqlResult.AddOutput("Security/Roles", roleResult.Name, roleResult.Sql);
                }
            }
        }

        private void AddSchemas(SqlResult sqlResult, IReadOnlyList<AtomModel> atoms, List<DatabaseRole> roles)
        {
            var schemaSet = new HashSet<string>(atoms.Where(a => a.AdditionalInfo.ShouldGenerateCode()).Select(a => a.AdditionalInfo.Schema));

            foreach (var schema in schemaSet)
            {
                sqlResult.AddOutput("Security/Schemas", schema, new SqlSchemaGenerator(schema, roles).Generate().Sql);
            }
        }

        private static bool ShouldCreate(AtomModel atomModel, QueryFlags flag)
        {
            return atomModel.HasFlag(flag);
        }

        private void AddCustomTableTypes(SqlResult sqlResult)
        {
            var tableTypes = MakeCustomTables();
            foreach (var tableType in tableTypes)
            {
                Log.Information("Adding default table type: {TableType}", tableType.Name);
                sqlResult.AddOutput("Types", tableType.Name, tableType.Sql);
            }
        }

        private CreateTableResult AddTable(SqlResult result, AtomModel atom)
        {
            Log.Information("Creating table: {TableName}", atom.Name);
            var table = CreateTable(atom);

            result.AddOutput("Tables", atom.AdditionalInfo.Schema + "." + atom.Name, table.TableSql);

            return table;
        }
    }
}
