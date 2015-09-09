using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;

namespace Atom.Generation.Generators.Code
{
    public class ProjectedAtomRoot
    {
        public string Name { get; set; }

        public string ProjectionGroup { get; set; }

        public List<AliasedAtomMemberInfo> Members { get; set; }

        public AtomModel BasedOn { get; set; }

        public static ProjectedAtomRoot FromAtom(AtomModel atomModel)
        {
            return new ProjectedAtomRoot
                   {
                       BasedOn = atomModel,
                       Name = atomModel.Name,
                       Members = atomModel.Members.Select(AliasedAtomMemberInfo.FromAtomMemberInfo)
                                     .ToList()
                   };
        }
    }
}
