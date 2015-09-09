using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Serilog;

namespace Atom.Generation.Generators.Sql.Tables
{
    internal class TableIndexDefinitions
    {
        private readonly AtomModel _data;

        public TableIndexDefinitions(AtomModel data)
        {
            _data = data;

        }

        public IEnumerable<string> GetDefinitions()
        {
            foreach (var index in _data.IndexOn.Values)
            {
                if (index.Unique)
                {
                    yield return UniqueCompoundKey(index);
                }
                else
                {
                    yield return CompoundIndex(index);
                }
            }

            HashSet<string> processedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var member in _data.Members.Where(i => i.Queryable || i.Groups?.Count > 0))
            {
                foreach (var group in GetAtomIndexMembers(member.Groups))
                {
                    if (!processedGroups.Contains(group.Name))
                    {
                        if (group.Unique)
                        {
                            yield return UniqueCompoundKey(group);
                        }
                        else
                        {
                            yield return CompoundIndex(group);
                        }

                        processedGroups.Add(group.Name);
                    }
                }

                if (member.IsAltKey ||
                    member.IsPrimary)
                {
                    continue;
                }

                if (member.Queryable)
                {
                    yield return BuildIndex(member);
                }
            }
        }

        private string BuildIndexColumnDef(AtomMemberInfo member)
        {
            return $"[{member.Name}] {GetIndexColumnDeclaration(member)}".Trim();
        }

        private string BuildIndex(AtomMemberInfo member)
        {
            string
                IndexType = (member.Unique ? "UNIQUE " : "") + "NONCLUSTERED",
                TableName = _data.Name,
                Columns = member.Name,
                Schema = _data.AdditionalInfo.Schema,
                IndexPrefix = member.Unique ? "AK" : "IX",
                SpacedColumns = BuildIndexColumnDef(member);

            return $@"CREATE {IndexType} INDEX [{IndexPrefix}_{TableName}_{Columns}] ON [{Schema}].[{TableName}] ({SpacedColumns}) ON [PRIMARY]";
        }

        private string GetIndexColumnDeclaration(AtomMemberInfo member)
        {
            if (!member.SortDirection.HasValue)
            {
                return string.Empty;
            }

            switch (member.SortDirection.Value)
            {
                case AtomMemberSortDirection.Asc:
                    return string.Empty;
                case AtomMemberSortDirection.Desc:
                    return "DESC";
            }

            string message = "Unknown index column declaration";

            Log.Error($"{message} {{@Member}}", member);
            throw new Exception(message);
        }

        private string UniqueCompoundKey(AtomIndexDefinition index)
        {
            var indexMembers = index.GetIndexMembers(_data);

            string
                IndexType = "UNIQUE NONCLUSTERED",
                TableName = _data.Name,
                Schema = _data.AdditionalInfo.Schema,
                IndexPrefix = "AK",
                Columns = string.Join("_", indexMembers.Select(i => i.Name)),
                SpacedColumns = string.Join(", ", indexMembers.Select(BuildIndexColumnDef));

            return $@"CREATE {IndexType} INDEX [{IndexPrefix}_{TableName}_{Columns}] ON [{Schema}].[{TableName}] ({SpacedColumns}) ON [PRIMARY]";
        }

        private string CompoundIndex(AtomIndexDefinition index)
        {
            var indexMembers = index.GetIndexMembers(_data);

            string
                IndexType = "NONCLUSTERED",
                TableName = _data.Name,
                Schema = _data.AdditionalInfo.Schema,
                IndexPrefix = "IX",
                Columns = string.Join("_", indexMembers.Select(i => i.Name)),
                SpacedColumns = string.Join(", ", indexMembers.Select(BuildIndexColumnDef));

            return $@"CREATE {IndexType} INDEX [{IndexPrefix}_{TableName}_{Columns}] ON [{Schema}].[{TableName}] ({SpacedColumns}) ON [PRIMARY]";
        }

        private IEnumerable<AtomIndexDefinition> GetAtomIndexMembers(List<string> groups)
        {
            if (groups == null || groups.Count == 0)
            {
                yield break;
            }

            var relatedGroups = _data.Members.Where(member => member.Groups?.Count > 0)
                                 .Where(member => groups.Any(member.Groups.Contains))
                                 .SelectMany(
                                     member => member.Groups.Select(
                                         groupName => new
                                         {
                                             member,
                                             groupName = groupName
                                         }))
                                 .GroupBy(i => i.groupName);

            foreach (var grouping in relatedGroups)
            {
                AtomGroupDefinition group;

                if (!_data.Groups.TryGetValue(grouping.Key, out group))
                {
                    continue;
                }

                var indexDef = new AtomIndexDefinition
                {
                    Unique = group.Unique,
                    Columns = grouping.Select(i => i.member.Name).ToList()
                };
            }
        }
    }
}
