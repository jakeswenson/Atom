using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Generation.Data;

namespace Atom.Generation.Generators.Sql.Projections
{
    internal class CustomQueryStoredProcedureGenerator
    {
        private readonly Dictionary<string, AtomModel> _allAtoms;

        public CustomQueryStoredProcedureGenerator(ProjectionAtom projection, IEnumerable<AtomModel> allAtoms)
        {
            Projection = projection;
            _allAtoms = allAtoms.ToDictionary(a => a.Name);
        }

        protected ProjectionAtom Projection { get; }

        internal ProjectionResult Generate()
        {
            var result = new ProjectionResult
            {

            };

            QueryPlanBuilder builder = new QueryPlanBuilder(Projection, _allAtoms);
            var plan = builder.Build();
            QuerySqlGenerator query = new QuerySqlGenerator(plan);
            query.Generate();
        }
    }
}