using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Atom.Data;
using Atom.Data.Projections;

namespace Atom.Generation.Generators.Sql.Projections
{
    public class ProjectionMemberGenerator
    {
        private readonly List<AtomModel> _allAtoms;

        public ProjectionMemberGenerator(List<AtomModel> allAtoms)
        {
            _allAtoms = allAtoms;
        }

        public IEnumerable<Code.ProjectedAtomRoot> GetProjections(IEnumerable<ProjectionAtom> projections)
        {
            foreach (var projection in projections)
            {
                var queryBuilder = new QueryPlanBuilder(projection, _allAtoms.Where(i => i.AdditionalInfo.Schema == Constants.DefaultSchema).ToDictionary(i => i.Name));

                var columns = queryBuilder.GetProjectionColumns();

                if (projection.IgnoreConflicts)
                {
                    columns = columns.DistinctBy(i => i.Name);
                }

                var references = columns.ToList();

                yield return new Code.ProjectedAtomRoot
                             {
                                 Name = projection.Name,
                                 Members = references
                             };
            }
        }
    }
}
