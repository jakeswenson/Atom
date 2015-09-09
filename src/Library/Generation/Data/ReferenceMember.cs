using Atom.Data;

namespace Atom.Generation.Data
{
    public class ReferenceMember
    {
        public Reference Target { get; set; }

        public AtomMemberInfo ThroughMember { get; set; }
    }
}