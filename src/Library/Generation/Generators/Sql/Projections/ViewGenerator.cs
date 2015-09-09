using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Generation;
using Atom.Generation.Data;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Generation.Generators.Sql.Projections
{
    public class ViewGenerator : ISqlGenerator<ProjectionResult>
    {
        private readonly ProjectionAtom _projectionAtom;

        private readonly Dictionary<string, AtomModel> _allAtoms;

        public ViewGenerator(ProjectionAtom projectionAtom, IEnumerable<AtomModel> allAtoms)
        {
            _projectionAtom = projectionAtom;
            _allAtoms = allAtoms.ToDictionary(a => a.Name);
        }

        public ProjectionResult Generate()
        {
            var queryPlanBuilder = new QueryPlanBuilder(_projectionAtom, _allAtoms);

            var queryPlan = queryPlanBuilder.Build();
            var viewName = $"view_{_projectionAtom.Name}";

            GenerateSql(viewName, queryPlan);

            return new ProjectionResult
                   {
                       References = queryPlan.References,
                       Meta = new SqlAccessorMetadata
                              {
                                  Name = viewName,
                                  BaseAtom = new ProjectedAtomRoot
                                                  {
                                                      Name = _projectionAtom.Name,
                                                      Members = queryPlan.QueryMembers.ToList()
                                                  },
                                  QueryType = QueryType.View
                              },
                       Sql = GenerateSql(viewName, queryPlan),
                       Name = Constants.DefaultSchema + "." + viewName
                   };
        }

        private string GenerateSql(string viewName, QueryPlan queryPlan)
        {
            string query = new QuerySqlGenerator(queryPlan, 1).Generate();

            return $@"
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO

CREATE VIEW [dbo].[{viewName}]
AS
	{query}

GO
";
        }
    }
}
