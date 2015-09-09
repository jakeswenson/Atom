using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Atom.Data
{
    public class AtomModel
    {
        private QueryFlags _queryFlags;

        public AtomModel()
        {
            Groups = new Dictionary<string, AtomGroupDefinition>(StringComparer.OrdinalIgnoreCase);
            IndexOn = new Dictionary<string, AtomIndexDefinition>(StringComparer.OrdinalIgnoreCase);
            Members = new OrderedAtomMembers();
            UpsertOn = new List<string>();
        }

        public string Name { get; set; }

        public AtomAdditionalInfo AdditionalInfo { get; set; }

        public OrderedAtomMembers Members { get; set; }

        public List<AtomReference> SearchableBy { get; set; }

        public bool IsLookup
        {
            get
            {
                return Lookup != null;
            }
        }

        public LookupDefinition Lookup { get; set; }

        public Dictionary<string, AtomGroupDefinition> Groups { get; set; }

        public Dictionary<string, AtomIndexDefinition> IndexOn { get; set; }

        public List<string> UpsertOn { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}";
        }

        public ReadOnlyCollection<string> GetDependencies()
        {
            return Members.Where(m => m.HasReference)
                          .Select(m => m.Reference.Name)
                          .ToList()
                          .AsReadOnly();
        }

        public IEnumerable<AtomMemberInfo> SelectMembers(IEnumerable<string> memberNames)
        {
            foreach (var memberName in memberNames)
            {
                yield return Members[memberName];
            }
        }

        internal bool HasFlag(QueryFlags flags)
        {
            return _queryFlags.HasFlag(flags);
        }

        internal void AddFlags(QueryFlags flags)
        {
            _queryFlags |= flags;
        }
    }
}
