using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;

namespace Atom.Generation.Data
{
    public class ResolvedReference : Reference
    {
        private readonly AtomModel _atom;

        private readonly IEnumerable<Reference> _resolvedReferences;

        public ResolvedReference(AtomModel atom, IEnumerable<Reference> resolvedReferences)
        {
            _atom = atom;
            _resolvedReferences = resolvedReferences;
            ReferencingMembers = ResolveMembers(atom, resolvedReferences).ToList();
        }

        private static IEnumerable<ReferenceMember> ResolveMembers(AtomModel atom, IEnumerable<Reference> resolvedReferences)
        {
            foreach (var resolvedReference in resolvedReferences)
            {
                var forwardReferenceMember = atom.Members.SingleOrDefault(m => m.HasReference && m.Reference?.Name == resolvedReference.Name);
                var backwardReferenceMember =
                    resolvedReference.ReferenceTarget.Members.SingleOrDefault(
                        m => m.HasReference && m.Reference?.Name == atom.Name);

                yield return new ReferenceMember
                {
                    Target = resolvedReference,
                    ThroughMember = forwardReferenceMember ?? backwardReferenceMember
                };
            }
        }

        public override AtomModel ReferenceTarget
        {
            get { return _atom; }
        }

        public IEnumerable<Reference> ResolvedReferences
        {
            get { return _resolvedReferences; }
        }

        public IEnumerable<ReferenceMember> ReferencingMembers { get; }

        public override string ToString() => $"{_atom.Name}({string.Join(", ", ResolvedReferences)})";
    }
}
