using System.Collections.Generic;
using System.IO;
using System.Linq;
using Atom.Data;
using Atom.Data.Serialization;
using Atom.Generation.Data;
using Atom.Generation.Generators.Code.CSharp;
using Atom.Generation.Generators.Sql;

namespace Atom.Generation.Generators.Code
{
    public class RepositoryGenerator : IGenerator<GeneratorResult, CSharpTargetConfig>
    {
        private readonly SqlResult _sqlGenerationResults;

        public RepositoryGenerator(SqlResult sqlGenerationResults)
        {
            _sqlGenerationResults = sqlGenerationResults;
        }

        public GeneratorResult Generate(GeneratorArguments<CSharpTargetConfig> generatorArguments)
        {
            var result = new GeneratorResult();

            var allAtoms = AtomCreator.FromFolder(generatorArguments.AtomsFolder);

            var repositoryMembers = _sqlGenerationResults.SqlAccessors.Select(sqlAccessorMetadata => ToRepoMember(sqlAccessorMetadata, allAtoms))
                                           .ToList();

            var singleRepoGenerator = new CSharpSingleRepositoryGenerator(generatorArguments.Config);

            var singleRepo = singleRepoGenerator.Generate(repositoryMembers);

            result.AddOutput(Path.ChangeExtension(singleRepo.Name, generatorArguments.Config.FileExtension), singleRepo.CodeString);

            var dbFile = singleRepoGenerator.GenerateDbFile(repositoryMembers);

            result.AddOutput(Path.ChangeExtension(dbFile.Name, generatorArguments.Config.FileExtension), dbFile.CodeString);

            return result;
        }

        private RepositoryMemberInfo ToRepoMember(SqlAccessorMetadata sqlAccessorMetadata, IReadOnlyCollection<AtomModel> allAtoms)
        {
            return new RepositoryMemberInfo
            {
                BaseAtomTypeName = sqlAccessorMetadata.BaseAtom.Name,
                Info = sqlAccessorMetadata
            };
        }
    }
}
