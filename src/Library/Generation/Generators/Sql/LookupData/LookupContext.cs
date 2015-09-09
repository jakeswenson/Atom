using System.Collections.Generic;
using System.Linq;
using Atom.Data;

namespace Atom.Generation.Generators.Sql.LookupData
{
    public class LookupContext
    {
        private readonly AtomModel _lookup;

        public LookupContext(AtomModel lookup)
        {
            _lookup = lookup;
            IdMember = _lookup.Members.First(i => i.IsPrimary);
            NameMember = _lookup.Members.First(i => i.Name.EndsWith("Name"));
            DescriptionMember = _lookup.Members.First(i => i.Name.EndsWith("Description"));
            CreatedOnMember = _lookup.Members.FirstOrDefault(m => m.HasFlag(MemberFlags.CreatedDateTimeTracking));
            LastModifiedMember = _lookup.Members.FirstOrDefault(m => m.HasFlag(MemberFlags.LastModifiedDateTimetracking));
            SoftDeleteMember = _lookup.Members.FirstOrDefault(m => m.HasFlag(MemberFlags.SoftDeleteTracking));
        }

        public string Schema
        {
            get
            {
                return _lookup.AdditionalInfo.Schema;
            }
        }

        public string TableName
        {
            get
            {
                return _lookup.Name;
            }
        }

        public AtomMemberInfo IdMember { get; }

        public AtomMemberInfo NameMember { get; }

        public AtomMemberInfo DescriptionMember { get; }

        public IEnumerable<AtomMemberInfo> OtherMembers
        {
            get
            {
                return Atom.Members.Where(m => !m.HasFlag(MemberFlags.Generated));
            }
        }

        public AtomMemberInfo LastModifiedMember { get; }

        public AtomMemberInfo CreatedOnMember { get; }

        public AtomMemberInfo SoftDeleteMember { get; }

        public AtomModel Atom
        {
            get
            {
                return _lookup;
            }
        }
    }
}