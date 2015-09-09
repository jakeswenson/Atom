using System;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpMemberNameFinder
    {
        private readonly AliasedAtomMemberInfo _member;

        public CSharpMemberNameFinder(AtomMemberInfo member)
            : this(AliasedAtomMemberInfo.FromAtomMemberInfo(member))
        {
        }

        public CSharpMemberNameFinder(AliasedAtomMemberInfo member)
        {
            _member = member;
        }

        public string MemberName()
        {
            if (_member.Member.HasReference &&
                _member.Member.Reference.TargetMember.Atom.IsLookup &&
                _member.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            {
                return _member.Name.Substring(0, _member.Name.Length - 2);
            }

            if (_member.Member.HasReference && _member.Member.Reference.IsReferenceToHiddenPrimaryKey)
            {
                return _member.Member.Reference.TargetAtomAlternateKey.Name;
            }

            return _member.Name;
        }
    }
}
