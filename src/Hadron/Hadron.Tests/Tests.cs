using System;
using HadronApplication.Options;
using Newtonsoft.Json;
using Atom.Data;
using Atom.Generation;
using Atom.Generation.Generators.Code;
using Xunit;
using Atom.Generation.Generators.Sql;
using Atom.Data.Serialization;

namespace HadronApplication.Tests
{
    public class Tests
    {
        [Fact]
        public void DeserializeAtomConfig()
        {
            var config = @"{
    ""atomPath"": ""UnitTests/TestAtoms/CareRelated"",
    ""targets"": {
        ""sql"": {
    		""path"" : ""sql/sql/Sample/RedGateScripts""
    	},
        ""c#"": {
			""path"" : ""src/Library"",    
            ""repo"": {
			    ""path"" : ""src/Library/Repos""
            }
        },
    }
}";
            var result = JsonConvert.DeserializeObject<AtomConfig>(config);
        }

        [Fact]
        public void Test()
        {
            var folder = @"..\UnitTests\TestAtoms\CareRelated";

            var result = new CodeGenerator().Generate(new GeneratorArguments<CSharpTargetConfig>(new CSharpTargetConfig(), folder, AtomCreator.LoadDefaults(folder)));
        }

        [Fact]
        public void TestStrongTypesCode()
        {
            var folder = @"..\UnitTests\TestAtoms\CareRelated";

            var result = new CodeGenerator().Generate(new GeneratorArguments<CSharpTargetConfig>(new CSharpTargetConfig()
                                                                                                         {
                                                                                                             Entities = { StrongTypes = true}
                                                                                                         }, folder, AtomCreator.LoadDefaults(folder)));

            foreach (var item in result)
            {
                Console.WriteLine(item.Output);
            }
        }

        [Fact]
        public void TestStrongTypesRepo()
        {
            var folder = @"..\UnitTests\TestAtoms\CareRelated";

            var args = new GeneratorArguments<CSharpTargetConfig>(
                new CSharpTargetConfig
                {
                    Entities =
                    {
                        StrongTypes = true
                    }
                },
                folder, AtomCreator.LoadDefaults(folder));

            var sqlArgs = new GeneratorArguments<SqlTargetConfig>(new SqlTargetConfig(), args.AtomsFolder, AtomCreator.LoadDefaults(folder));
            var sqlGenerator = new SqlGenerator().Generate(sqlArgs);


            var result = new RepositoryGenerator(sqlGenerator).Generate(args);

            foreach (var item in result)
            {
                Console.WriteLine(item.Output);
            }
        }

        [Fact]
        public void TestRepos()
        {
            var folder = @"..\UnitTests\TestAtoms\CareRelated";
            var args = new GeneratorArguments<CSharpTargetConfig>(new CSharpTargetConfig(), folder, AtomCreator.LoadDefaults(folder));

            var sqlArgs = new GeneratorArguments<SqlTargetConfig>(new SqlTargetConfig(), args.AtomsFolder, AtomCreator.LoadDefaults(folder));
            var sqlGenerator = new SqlGenerator().Generate(sqlArgs);

            var result = new RepositoryGenerator(sqlGenerator).Generate(args);

            foreach (var item in result)
            {
                Console.WriteLine(item.Output);
            }
        }
    }
}
