using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Serialization;
using Atom.Generation.Generators.Code.CSharp;
using Atom.Generation.Generators.Sql.Projections;

namespace Atom.Generation.Generators.Code
{
    public class CodeGenerator : IGenerator<GeneratorResult, CSharpTargetConfig>
    {
        public GeneratorResult Generate(GeneratorArguments<CSharpTargetConfig> generatorArguments)
        {
            var result = new GeneratorResult();

            var generator = new CSharpCodeClassGenerator(generatorArguments.Config);

            var atoms = GetProjectionAtoms(generatorArguments, generatorArguments.Config);

            foreach (var generatorOutput in generator.Generate(atoms))
            {
                result.AddOutput(generatorOutput);
            }

            return result;
        }

        private IEnumerable<ProjectedAtomRoot> GetProjectionAtoms(GeneratorArguments<CSharpTargetConfig> generatorArguments, CSharpTargetConfig config)
        {
            var atoms = AtomCreator.FromFolder(generatorArguments.AtomsFolder, generatorArguments.Defaults)
                                   .ToList();

            var generator = new ProjectionMemberGenerator(atoms);

            var projectedAtoms = generator.GetProjections(AtomCreator.ProjectionsFromFolder(generatorArguments.AtomsFolder));

            return atoms.Select(ProjectedAtomRoot.FromAtom)
                        .Concat(projectedAtoms);
        }
    }
}
