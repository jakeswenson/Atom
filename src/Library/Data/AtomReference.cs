using System.Linq;

namespace Atom.Data
{
    public class AtomReference
    {
        public string Name { get; set; }

        public string Member { get; set; }

        public string Schema { get; set; }

        public bool Access { get; set; }

        internal AtomMemberInfo TargetMember { get; set; }

        internal bool IsReferenceToHiddenPrimaryKey { get { return TargetMember.IsPrimary && TargetMember.HasFlag(MemberFlags.HiddenId); } }

        internal AtomMemberInfo TargetAtomAlternateKey { get { return TargetMember.Atom.Members.Single(m => m.Key); } }
    }
}
