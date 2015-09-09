using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Atom.Data;
using Atom.Data.Serialization;
using Atom.Generation;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Sql;
using CommandLine;
using Newtonsoft.Json;
using Serilog;

namespace HadronApplication.Options.Verbs
{
    public class GenerationVerb : Verb
    {
        public GenerationVerb()
        {
        }

        [Option('c', Required = false, HelpText = "The path to the config file", DefaultValue = ".atom.config")]
        public string AtomConfigPath { get; set; }

        private AtomConfig TryLoadAtomConfig()
        {
            AtomConfigPath = Path.GetFullPath(AtomConfigPath);

            if (File.Exists(AtomConfigPath))
            {
                Log.Information("Found atom config");

                var configContents = File.ReadAllText(AtomConfigPath).Trim();
                var config = JsonConvert.DeserializeObject<AtomConfig>(configContents);

                Log.Debug("Loaded config {@AtomConfig}", config);

                string basePath = Path.GetDirectoryName(Path.GetFullPath(AtomConfigPath));

                foreach (var item in config.Targets.Values)
                {
                    var sqlTargetConfig = item as SqlTargetConfig;
                    if (sqlTargetConfig != null)
                    {
                        sqlTargetConfig.MakeAbsolutePath(basePath);
                    }
                    else
                    {
                        var cSharpTargetConfig = item as CSharpTargetConfig;
                        if (cSharpTargetConfig != null)
                        {
                            cSharpTargetConfig.Entities.MakeAbsolutePath(basePath);
                            cSharpTargetConfig.Repository.MakeAbsolutePath(basePath);
                        }
                    }
                }

                return config;
            }
            else
            {
                Log.Error("No atom config found in at: {AtomConfigSearchPath}", AtomConfigPath);
                return null;
            }
        }

        protected override IEnumerable<GeneratorResult> Generate()
        {
            AtomConfig config = TryLoadAtomConfig();

            if (config == null)
            {
                yield break;
            }

            var sqlConfig = config.Targets.Values.OfType<SqlTargetConfig>().Single();
            var csharpConfig = config.Targets.Values.OfType<CSharpTargetConfig>().Single();

            var defaults = AtomCreator.LoadDefaults(config.AtomPath);

            var sqlArgs = new GeneratorArguments<SqlTargetConfig>(sqlConfig, config.AtomPath, defaults);
            var args = new GeneratorArguments<CSharpTargetConfig>(csharpConfig, config.AtomPath, defaults, sqlArgs.Atoms);

            var sqlResult = new SqlGenerator().Generate(sqlArgs);

            sqlResult.OutputPath = sqlConfig.OutputPath;

            //if (Directory.Exists(sqlResult.OutputPath))
            //{
            //    foreach (var file in Directory.EnumerateFiles(sqlResult.OutputPath, "*.generated.sql", SearchOption.AllDirectories))
            //    {
            //        sqlResult.Deletions.Add(file);
            //    }
            //}

            string redGateFile = Path.Combine(sqlConfig.OutputPath, "RedGateDatabaseInfo.xml");
            Log.Information("Looking for redgate database info xml at {RedGateDatabaseInfoXml}", redGateFile);


            if (sqlResult.DataFiles.Any() &&
                File.Exists(redGateFile))
            {
                Log.Information("Found redgate file, updating...");

                var regate = XDocument.Load(uri: redGateFile);
                var dataFiles = regate.Root.Elements("DataFileSet").First();
                var allDataFiles = dataFiles.Elements("DataFile")
                                            .Select(n => n.Value).Union(sqlResult.DataFiles, StringComparer.OrdinalIgnoreCase).ToList();

                dataFiles.Elements("Count")
                         .First()
                         .Value = allDataFiles.Count().ToString();

                Log.Information("DataFiles Count: {DataFileCount}", allDataFiles.Count());

                dataFiles.Elements("DataFile").Remove();

                dataFiles.Add(allDataFiles.Select(s => new XElement("DataFile", s)));

                var genResult = new GeneratorResult()
                {
                    OutputPath = sqlConfig.OutputPath
                };

                genResult.AddOutput("RedGateDatabaseInfo.xml", regate.ToString());

                yield return genResult;
            }

            yield return sqlResult;

            if (csharpConfig.Repository != null)
            {
                var repoResult = new RepositoryGenerator(sqlResult).Generate(args);

                repoResult.OutputPath = csharpConfig.Repository.OutputPath;

                yield return repoResult;
            }

            var codeResult = new CodeGenerator().Generate(args);

            codeResult.OutputPath = csharpConfig.Entities.OutputPath;

            yield return codeResult;
        }
    }
}