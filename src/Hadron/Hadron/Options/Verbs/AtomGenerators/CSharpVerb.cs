using System.Collections.Generic;
using CommandLine;
using Atom.Data;
using Atom.Generation;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Sql;
using System;
using Atom.Data.Serialization;

namespace HadronApplication.Options.Verbs
{
    public class CSharpVerb : AtomGenerationVerb
    {
        private readonly CSharpTargetConfig _cSharpTargetConfig;

        public CSharpVerb()
        {
        }

        public CSharpVerb(string atomsPath, CSharpTargetConfig cSharpTargetConfig)
        {
            AtomsFolder = atomsPath;
            _cSharpTargetConfig = cSharpTargetConfig;
        }

        [Option('n', "namespace", DefaultValue = "PF.DataAccess.Records", HelpText = "The data record namespace")]
        public string DataNamespace { get; set; }
    
        [Option('r', "repo", HelpText = "Generate repo")]
        public bool GenerateRepo { get; set; }

        [Option("repo-output-path", HelpText = "The output path of the cs files doing reposotiry access")]
        public string RepoOutputPath { get; set; }

        [Option("strong-types", HelpText = "The generates strong types")]
        public bool StrongTypes { get; set; }

        [Option("repo-namespace", DefaultValue = "PF.DataAccess.Repositories", HelpText = "The sql repo namespace")]
        public string RepoNamespace { get; set; }

        protected override IEnumerable<GeneratorResult> Generate()
        {            
            return FromTargetConfigs(GetConfig());
        }

        private CSharpTargetConfig GetConfig()
        {
            if (_cSharpTargetConfig != null)
            {
                return _cSharpTargetConfig;
            }

            var targetConfig = new CSharpTargetConfig
            {
                Entities =
                {
                    Namespace = DataNamespace,
                    OutputPath = OutputPath,
                    StrongTypes = StrongTypes
                }
            };

            if (GenerateRepo)
            {
                targetConfig.Repository = new RepositoryTargetConfiguration
                {
                    OutputPath = RepoOutputPath ?? OutputPath,
                    Namespace = RepoNamespace
                };
            }

            return targetConfig;
        }

        private IEnumerable<GeneratorResult> FromTargetConfigs(CSharpTargetConfig targetConfig)
        {
            var args = new GeneratorArguments<CSharpTargetConfig>(targetConfig, AtomsFolder, AtomCreator.LoadDefaults(AtomsFolder));

            if(targetConfig.Repository != null){
                var sqlArgs = new GeneratorArguments<SqlTargetConfig>(new SqlTargetConfig(), args.AtomsFolder, AtomCreator.LoadDefaults(AtomsFolder));
                var sqlGenerator = new SqlGenerator().Generate(sqlArgs);

                var repoResult = new RepositoryGenerator(sqlGenerator).Generate(args);

                repoResult.OutputPath = targetConfig.Repository.OutputPath;

                yield return repoResult;
            }

            var codeResult = new CodeGenerator().Generate(args);

            codeResult.OutputPath = targetConfig.Entities.OutputPath;

            yield return codeResult;
        }
    }
}
