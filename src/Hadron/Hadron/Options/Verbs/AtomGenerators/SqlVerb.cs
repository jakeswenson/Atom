using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Atom;
using Atom.Data;
using Atom.Data.Serialization;
using Atom.Generation;
using Atom.Generation.Data;
using Atom.Generation.Generators.Sql;
using Serilog;

namespace HadronApplication.Options.Verbs.AtomGenerators
{
    public class SqlVerb : AtomGenerationVerb
    {
        private readonly SqlTargetConfig _config;

        public SqlVerb(string atomsPath, SqlTargetConfig config)
        {
            _config = config;

            AtomsFolder = atomsPath;
        }

        public SqlVerb()
        {
        }

        protected override IEnumerable<GeneratorResult> Generate()
        {
            var sqlConfig = _config ?? new SqlTargetConfig
                            {
                                OutputPath = OutputPath
                            };

            return FromTargetConfigs(sqlConfig);
        }

        private IEnumerable<GeneratorResult> FromTargetConfigs(SqlTargetConfig sqlConfig)
        {
            var args = new GeneratorArguments<SqlTargetConfig>(sqlConfig, AtomsFolder, AtomCreator.LoadDefaults(AtomsFolder));

            var sqlResult = new SqlGenerator().Generate(args);

            sqlResult.OutputPath = sqlConfig.OutputPath;

            if (Directory.Exists(sqlResult.OutputPath))
            {
                foreach (var file in Directory.EnumerateFiles(sqlResult.OutputPath, "*.generated.sql", SearchOption.AllDirectories))
                {
                    sqlResult.Deletions.Add(file);
                }
            }

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
                    OutputPath = _config.OutputPath
                };

                genResult.AddOutput("RedGateDatabaseInfo.xml", regate.ToString());

                yield return genResult;
            }

            yield return sqlResult;
        }
    }
}
