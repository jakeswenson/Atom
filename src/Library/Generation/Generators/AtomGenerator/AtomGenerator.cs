using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Atom.Data;
using Atom.Generation.Data;
using Serilog;

namespace Atom.Generation.Generators.AtomGenerator
{
    public class AtomGenerator : IGenerator<GeneratorResult, AtomGenerationTargetConfig>
    {
        private GeneratorArguments<AtomGenerationTargetConfig> _config;

        public GeneratorResult Generate(GeneratorArguments<AtomGenerationTargetConfig> generatorArguments)
        {
            _config = generatorArguments;

            Log.Information("Beginning reverse atom genernation {@Config}", new { generatorArguments.Config });

            var server = new Server(generatorArguments.Config.Host);

            var db = server.Databases[generatorArguments.Config.Database];

            var result = new GeneratorResult();

            foreach (Table table in db.Tables)
            {
                Log.Information("Processing: {Name} {Schema}", table.Name, table.Schema);
                result.AddOutput(CreateOutput(db, table));
            }

            return result;
        }

        private GeneratorOutput CreateOutput(Database db, Table table)
        {
            var atomRoot = CreateAtomRoot(db, table);

            var settings = new JsonSerializerSettings
                           {
                               DefaultValueHandling = DefaultValueHandling.Ignore,
                               NullValueHandling = NullValueHandling.Ignore,
                               Formatting = Formatting.Indented,
                               ContractResolver = new CamelCasePropertyNamesContractResolver()
                           };

            var outputFileName = $"{atomRoot.Name}.atom";

            if (atomRoot.AdditionalInfo.Schema != Constants.DefaultSchema)
            {
                outputFileName = atomRoot.AdditionalInfo.Schema + "." + outputFileName;
            }

            return new GeneratorOutput(outputFileName, JsonConvert.SerializeObject(atomRoot, settings));
        }

        private AtomModel CreateAtomRoot(Database db, Table table)
        {
            var atomRoot = new AtomModel();

            if (db.DefaultSchema != table.Schema)
            {
                atomRoot.AdditionalInfo.Schema = table.Schema;
            }

            atomRoot.Name = table.Name;

            atomRoot.Members = new OrderedAtomMembers();

            atomRoot.AdditionalInfo = new AtomAdditionalInfo();

            var memberList = table.Columns.Cast<Column>()
                                  .Select(c => GetMember(db, table, c))
                                  .ToList();

            memberList = PostProcessRawMemberList(atomRoot, memberList);

            memberList.ForEach(atomRoot.Members.Add);

            AssignCompoundKeys(db, table, atomRoot, memberList);

            return atomRoot;
        }

        private void AssignCompoundKeys(Database db, Table table, AtomModel atomModel, List<AtomMemberInfo> memberList)
        {
            var memberLookup = memberList.ToDictionary(i => i.Name);

            var compoundIndexes = table.Indexes.Cast<Index>()
                                       .Where(
                                           i => i.IndexedColumns.Cast<IndexedColumn>()
                                                 .Count() > 1)
                                       .ToList();

            bool? hasItems = !compoundIndexes?.Any();

            if (hasItems.GetValueOrDefault())
            {
                foreach (var compoundIndex in compoundIndexes)
                {
                    if (compoundIndex.IndexKeyType == IndexKeyType.DriPrimaryKey)
                    {
                        // skip compound primary keys since they will already be created with individual key constraints

                        continue;
                    }
                    var group = compoundIndex.Name;

                    foreach (IndexedColumn column in compoundIndex.IndexedColumns)
                    {
                        var member = memberLookup[column.Name];

                        if (member.Groups == null)
                        {
                            member.Groups = new List<string>();
                        }

                        member.Groups.Add(group);
                    }

                    atomModel.Groups.Add(
                        group,
                        new AtomGroupDefinition
                        {
                            Name = group,
                            Unique = compoundIndex.IsUnique
                        });
                }
            }
        }

        private List<AtomMemberInfo> PostProcessRawMemberList(AtomModel atomModel, List<AtomMemberInfo> memberList)
        {
            if (_config.Config.InferrHiddenKey)
            {
                memberList = InferHiddenKeys(atomModel, memberList);
            }

            memberList = HandleExplicitTemporal(atomModel, memberList).ToList();

            memberList = HandleExplicitIsDeleted(atomModel, memberList);

            return memberList;
        }

        private List<AtomMemberInfo> InferHiddenKeys(AtomModel atomModel, List<AtomMemberInfo> memberList)
        {
            var alts = memberList.Where(i => i.Name.Equals(atomModel.Name + "guid", StringComparison.OrdinalIgnoreCase))
                                 .ToList();

            if (alts.Count == 1)
            {
                var primaryKey = memberList.Where(i => i.IsPrimary);

                atomModel.AdditionalInfo.HideId = true;

                alts[0].IsAltKey = true;

                return memberList.Except(primaryKey)
                                 .ToList();
            }

            return memberList;
        }

        private List<AtomMemberInfo> HandleExplicitIsDeleted(AtomModel atomModel, List<AtomMemberInfo> memberList)
        {
            var byName = memberList.ToDictionary(i => i.Name);

            if (byName.ContainsKey(Constants.Members.IsDeleted))
            {
                atomModel.AdditionalInfo.UseSoftDeletes = true;

                return memberList.Where(i => !i.Name.Equals(Constants.Members.IsDeleted, StringComparison.OrdinalIgnoreCase))
                                 .ToList();
            }

            return memberList;
        }

        private List<AtomMemberInfo> HandleExplicitTemporal(AtomModel atomModel, List<AtomMemberInfo> memberList)
        {
            var byName = memberList.ToDictionary(i => i.Name);

            var createdOnUtcMember = Constants.CreatedOnDateTimePotentialNames.FirstOrDefault(byName.ContainsKey);
            var lastModifiedOnUtcMember = Constants.LastModifiedOnOnDateTimePotentialNames.FirstOrDefault(byName.ContainsKey);

            if (createdOnUtcMember != null &&
                lastModifiedOnUtcMember != null)
            {
                atomModel.AdditionalInfo.Temporal.HasTemporal = true;
                
                if (byName[createdOnUtcMember].Queryable)
                {
                    atomModel.AdditionalInfo.Temporal.CreatedOnSort = byName[createdOnUtcMember].SortDirection ?? AtomMemberSortDirection.Asc;
                }
                else if (byName[lastModifiedOnUtcMember].Queryable)
                {
                    atomModel.AdditionalInfo.Temporal.LastModifiedSort = byName[lastModifiedOnUtcMember].SortDirection ?? AtomMemberSortDirection.Asc;
                }

                return memberList.Where(i => !(i.Name.Equals(createdOnUtcMember) || i.Name.Equals(lastModifiedOnUtcMember)))
                                 .ToList();
            }

            return memberList;
        }

        private AtomMemberInfo GetMember(Database db, Table table, Column column)
        {
            return new AtomMemberInfo
                   {
                       IsPrimary = column.InPrimaryKey,
                       Name = column.Name,
                       Optional = column.Nullable,
                       Type = InferrableType(column) ? null : GetType(column.DataType),
                       Length = GetLength(column.DataType),
                       Unique = IsUnique(column, table),
                       Unicode = IsUnicode(column),
                       IsAltKey = IsAltKey(column, table),
                       Queryable = HasIndex(column, table),
                       Reference = ReferencesOthers(db, column, table),
                       DefaultValue = GetDefaultValue(column),
                       SortDirection = GetSortDirection(column, table)
                   };
        }

        private AtomMemberSortDirection? GetSortDirection(Column column, Table table)
        {
            var indexes = GetSingularIndexes(column, table)
                .SelectMany(i => i.IndexedColumns.Cast<IndexedColumn>())
                .Where(i => i.Name == column.Name && !column.InPrimaryKey && i.Descending)
                .ToList();

            if (indexes?.Any() ?? true)
            {
                return null;
            }

            return AtomMemberSortDirection.Desc;            
        }

        private bool InferrableType(Column column)
        {
            return column.Name.EndsWith("Guid") || column.Name.EndsWith("Id");
        }

        private string GetDefaultValue(Column column)
        {
            var defaultValue = column.Default?.Any() ?? false ? null : column.Default;

            if (!string.IsNullOrEmpty(defaultValue))
            {
                return defaultValue;
            }

            if (column.DefaultConstraint != null)
            {
                return new ColumnDefaultParser(column).Default;
            }

            return null;
        }

        private AtomReference ReferencesOthers(Database db, Column column, Table table)
        {
            var indexes = table.ForeignKeys.Cast<ForeignKey>()
                               .ToList();

            var index = indexes.FirstOrDefault(i => i.Columns.Contains(column.Name));

            if (index != null &&
                index.ReferencedTable != table.Name)
            {
                var reference = new AtomReference
                                {
                                    Name = index.ReferencedTable,
                                    Schema = index.ReferencedTableSchema != table.Schema ? index.ReferencedTableSchema : null
                                };

                // find the related column

                Log.Debug("Looking for key {ReferencedKey} {ReferencedTable} {TableName} {ColumnName}",
                    index.ReferencedKey,
                    index.ReferencedTable,
                    table.Name,
                    column.Name);

                var referencedKey = 
                    db.Tables.Cast<Table>()
                        .First(i => i.Schema == index.ReferencedTableSchema && i.Name == index.ReferencedTable)
                        .Indexes.Cast<Index>()
                        .First(i => i.Name == index.ReferencedKey)
                        .IndexedColumns.Cast<IndexedColumn>()
                        .FirstOrDefault();

                if (referencedKey != null &&
                    (referencedKey.Name != column.Name))
                {
                    reference.Member = referencedKey.Name;
                }

                return reference;
            }

            return null;
        }

        private bool HasIndex(Column column, Table table)
        {
            return !(GetSingularIndexes(column, table)?.Any() ?? false);
        }

        private IEnumerable<Index> GetCompoundIndexes(Column column, Table table)
        {
            return table.Indexes.Cast<Index>()
                        .Where(i => i.IndexedColumns.Count > 1 && i.IndexedColumns.Contains(column.Name));
        }

        private IEnumerable<Index> GetSingularIndexes(Column column, Table table)
        {
            return table.Indexes.Cast<Index>()
                        .Where(i => i.IndexedColumns.Count == 1 && i.IndexedColumns.Contains(column.Name));
        }

        private bool IsAltKey(Column column, Table table)
        {
            return column.Name.Equals(table.Name + "Guid", StringComparison.OrdinalIgnoreCase) && !column.InPrimaryKey;
        }

        private bool? IsUnicode(Column column)
        {
            return column.DataType.SqlDataType == SqlDataType.NVarChar ? true : (bool?)null;
        }

        private bool IsUnique(Column column, Table table)
        {
            return GetSingularIndexes(column, table)
                .Any(i => i.IsUnique);
        }

        private int? GetLength(DataType dataType)
        {
            return IsString(dataType) ? (int?)dataType.MaximumLength : null;
        }

        private bool IsString(DataType dataType)
        {
            switch (dataType.SqlDataType)
            {
                case SqlDataType.NChar:
                case SqlDataType.NText:
                case SqlDataType.NVarChar:
                case SqlDataType.NVarCharMax:
                case SqlDataType.Text:
                case SqlDataType.VarBinary:
                case SqlDataType.VarBinaryMax:
                case SqlDataType.VarChar:
                case SqlDataType.VarCharMax:
                case SqlDataType.Variant:
                case SqlDataType.Xml:
                    return true;
            }

            return false;
        }

        private string GetType(DataType dataType)
        {
            switch (dataType.SqlDataType)
            {
                case SqlDataType.BigInt:
                    return "long";
                case SqlDataType.Bit:
                    return "bool";
            }

            if (IsString(dataType))
            {
                return "string";
            }

            return dataType.Name;
        }
    }
}
