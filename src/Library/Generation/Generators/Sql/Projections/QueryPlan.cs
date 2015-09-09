using System.Collections.Generic;
using Atom.Data.Projections;
using Atom.Generation.Data;

namespace Atom.Generation.Generators.Sql.Projections
{
    public class QueryPlan
    {
        public QueryPlan()
        {
            References = new List<Reference>();
            Filters = new List<AtomFilter>();
        }

        public IEnumerable<AliasedAtomMemberInfo> QueryMembers { get; set; }

        public List<Reference> References { get; set; }

        public List<AtomFilter> Filters { get; set; }
    }
}
