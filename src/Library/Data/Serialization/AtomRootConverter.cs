using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data.Types;

namespace Atom.Data.Serialization
{
    public class AtomRootConverter : JsonCreationConverter<AtomModel>
    {
        private readonly AtomAdditionalInfo _defaults;

        public static readonly Dictionary<string, Func<AtomMemberInfo, MemberType>> TypeFactories =
            new Dictionary<string, Func<AtomMemberInfo, MemberType>>(StringComparer.OrdinalIgnoreCase)
            {
                { "guid", mem => new MemberGuid() },
                { "long", mem => new MemberLong() },
                { "date", mem => new MemberDate() },
                { "datetime", mem => new MemberDateTime(useDateTime2: false) },
                { "datetime2", mem => new MemberDateTime(useDateTime2: true, dateTime2Precision: mem.Precision ?? 7) },
                { "string", mem => new MemberText(mem.Length ?? 100, unicode: mem.Unicode ?? false) },
                { "description", mem => new MemberText(mem.Length ?? 200, unicode: mem.Unicode ?? true) },
                { "name", mem => new MemberText(mem.Length ?? 100, mem.Unicode ?? true) },
                { "bool", mem => new MemberBool() },
                { "float", mem => new MemberFloat() },
                { "binary", mem => new MemberBinary(mem.Length ?? 8000, variableSize: mem.VariableSize) },
                { "double", mem => new MemberDouble() },
                { "byte", mem => new MemberByte() },
                { "short", mem => new MemberShort() },
                { "decimal", mem => new MemberDecimal() },
            };

        public AtomRootConverter(AtomAdditionalInfo defaults)
        {
            _defaults = defaults;
        }

        /// <summary>
        /// Takes information in the additional info block
        /// and updates members with it
        /// </summary>
        /// <param name="data"></param>
        public static void ApplyAdditionalInfoToMembers(AtomModel data)
        {
            if (data.AdditionalInfo.ShouldHidePrimaryKey())
            {
                data.Members.Insert(0, AtomMembers.Long(data.Name + "Id")
                                             .Key()
                                             .Generated()
                                             .Flag(MemberFlags.HiddenId));
            }

            if (data.AdditionalInfo.AutoGenId.GetValueOrDefault())
            {
                data.Members.Insert(0, AtomMembers.Long(data.Name + "Id")
                                             .Key()
                                             .Generated());
            }

            if (data.AdditionalInfo.Temporal.HasTemporal ?? false)
            {
                if (data.AdditionalInfo.Temporal.CreatedOn.GetValueOrDefault(true))
                {
                    var created = BuildDateTimeMemberInfo(data.AdditionalInfo.Temporal, Constants.Members.CreatedAtDateTime)
                                    .NotUpdateable()
                                    .Flag(MemberFlags.CreatedDateTimeTracking)
                                    .Generated();

                    if (data.AdditionalInfo.Temporal.IndexCreatedOn ?? false)
                    {
                        created.Queryable = true;
                    }

                    created.SortDirection = data.AdditionalInfo.Temporal.CreatedOnSort;
                    data.Members.Add(created);
                }

                if (data.AdditionalInfo.Temporal.LastModified.GetValueOrDefault(true))
                {
                    var lastModified = BuildDateTimeMemberInfo(data.AdditionalInfo.Temporal, Constants.Members.LastModifiedDateTime)
                                        .NotUpdateable()
                                        .Flag(MemberFlags.LastModifiedDateTimetracking)
                                        .Generated();

                    if (data.AdditionalInfo.Temporal.IndexLastModified ?? false)
                    {
                        lastModified.Queryable = true;
                    }

                    lastModified.SortDirection = data.AdditionalInfo.Temporal.LastModifiedSort;
                    data.Members.Add(lastModified);
                }
            }

            if (data.AdditionalInfo.UseSoftDeletes ?? false)
            {
                data.Members.Add(
                    AtomMembers.Bool(Constants.Members.IsDeleted)
                               .WithDefault(new BoolDefault(false))
                               .Flag(MemberFlags.SoftDeleteTracking)
                               .Generated());
            }
        }

        private static AtomMemberInfo BuildDateTimeMemberInfo(TemporalInfo temporalInfo, string name)
        {
            return temporalInfo.UseDateTime2.GetValueOrDefault(false)
                ? AtomMembers.DateTime2(name, temporalInfo.DateTime2Precision)
                : AtomMembers.DateTime(name);
        }

        protected override AtomModel After(AtomModel atom)
        {
            if (atom.IsLookup)
            {
                AddLookupMembers(atom);
                ProcessLookupValues(atom);
            }

            SetAdditionalInfo(atom);

            ApplyAdditionalInfoToMembers(atom);

            ThrowIfNowMembers(atom);

            SetupQueryFlags(atom);

            InitializeMembers(atom);

            SetupIndexNames(atom);

            return atom;
        }

        private void ProcessLookupValues(AtomModel atom)
        {
            atom.Lookup.Values[0].Index = atom.Lookup.Values[0].Index ?? 1;

            var values = atom.Lookup.Values.Zip(atom.Lookup.Values.Skip(1), (prev, current) => new { prev, current });

            foreach (var value in values)
            {
                value.current.Index = value.current.Index ?? value.prev.Index + 1;
            }
        }

        private void InitializeMembers(AtomModel atom)
        {
            foreach (var member in atom.Members)
            {
                member.MemberType = SelectMemberType(member);
                member.Atom = atom;
            }
        }

        private static void SetupIndexNames(AtomModel atom)
        {
            foreach (var group in atom.Groups)
            {
                @group.Value.Name = @group.Key;
            }

            foreach (var index in atom.IndexOn)
            {
                index.Value.Name = index.Key;
            }
        }

        private static void SetupQueryFlags(AtomModel data)
        {
            if (data.AdditionalInfo.QueryTypes.Count == 0)
            {
                data.AddFlags(QueryFlags.Defaults);
            }
            else
            {
                data.AdditionalInfo.QueryTypes.ForEach(data.AddFlags);
            }
        }

        private static void ThrowIfNowMembers(AtomModel data)
        {
            if (data.Members?.Count == 0)
            {
                throw new Exception($"{data.Name} has no members");
            }
        }

        private void AddLookupMembers(AtomModel data)
        {
            if (data.AdditionalInfo == null)
            {
                data.AdditionalInfo = new AtomAdditionalInfo();
            }

            if (data.Members == null)
            {
                data.Members = new OrderedAtomMembers();
            }

            var idMember = new AtomMemberInfo()
            {
                Name = data.Name + "Id",
                Type = data.Lookup.Type ?? "long"
            };

            var id = idMember.Generated().Key();

            var name = new AtomMemberInfo
            {
                Name = data.Name + "Name",
                Key = true
            }.Generated();

            var description = new AtomMemberInfo
            {
                Name = data.Name + "Description"
            }.Generated();

            var defaultLookupSettings = new AtomAdditionalInfo
            {
                Temporal = {
                                                HasTemporal = true
                                            },
                HideId = false,
                AutoGenId = false,
                QueryTypes = new List<QueryFlags> { QueryFlags.None }
            };

            defaultLookupSettings.Merge(_defaults);

            data.AdditionalInfo.Merge(defaultLookupSettings);

            data.Members.Insert(0, id);
            data.Members.Insert(1, name);
            data.Members.Insert(2, description);
        }

        private void SetAdditionalInfo(AtomModel data)
        {
            if (data.AdditionalInfo != null &&
                _defaults != null)
            {
                data.AdditionalInfo.Merge(_defaults);
            }
            else if (data.AdditionalInfo == null && _defaults != null)
            {
                data.AdditionalInfo = new AtomAdditionalInfo();
                data.AdditionalInfo.Merge(_defaults);
            }
            else if (data.AdditionalInfo == null)
            {
                data.AdditionalInfo = new AtomAdditionalInfo();
            }

            if (data.AdditionalInfo.Temporal == null)
            {
                data.AdditionalInfo.Temporal = new TemporalInfo();
            }
        }

        public MemberType SelectMemberType(AtomMemberInfo memberInfo)
        {
            if (memberInfo.MemberType != null)
            {
                return memberInfo.MemberType;
            }

            if (memberInfo.HasReference)
            {
                return new ReferenceMemberType();
            }

            string name = memberInfo.Name;

            Func<string, bool> nameEndsWith = ending => name.EndsWith(ending, comparisonType: StringComparison.OrdinalIgnoreCase);
            Func<string, bool> nameStartsWith = ending => name.StartsWith(ending, comparisonType: StringComparison.OrdinalIgnoreCase);

            bool hasNoTypeSpecified = string.IsNullOrWhiteSpace(memberInfo.Type);

            if (hasNoTypeSpecified)
            {
                if (nameEndsWith("utc"))
                {
                    return new MemberDateTime(useDateTime2: false);
                }

                if (nameEndsWith("guid"))
                {
                    return new MemberGuid();
                }

                if (nameStartsWith("is"))
                {
                    return new MemberBool();
                }

                if (nameEndsWith("id"))
                {
                    return new MemberLong();
                }

                if (nameEndsWith("name"))
                {
                    return new MemberText(memberInfo.Length ?? 100, unicode: memberInfo.Unicode ?? false);
                }

                if (nameEndsWith("description"))
                {
                    return new MemberText(memberInfo.Length ?? 255, unicode: memberInfo.Unicode ?? true);
                }
            }

            Func<AtomMemberInfo, MemberType> typeFunc;

            if (memberInfo.Type == null ||
                !TypeFactories.TryGetValue(memberInfo.Type, out typeFunc))
            {
                throw new Exception($"{memberInfo.Type} is an unknown generatable type for member {memberInfo.Name}");
            }

            return typeFunc(memberInfo);
        }
    }
}
