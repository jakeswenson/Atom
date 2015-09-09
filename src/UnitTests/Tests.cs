using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Data.Serialization;
using Atom.Generation;
using Atom.Generation.Data;
using Atom.Generation.Generators.AtomGenerator;
using Atom.Generation.Generators.Sql;
using Atom.Generation.Generators.Sql.Sprocs;
using Atom.Generation.Generators.Sql.Tables;
using Atom.Generation.Generators.Sql.Projections;
using Serilog;
using Xunit;
using Newtonsoft.Json;

namespace Atom.UnitTests
{
    public class ReverseAtomGenerationTests : TestBase
    {
        [Fact]
        public void Generates()
        {
            var results = new AtomGenerator().Generate(
                new GeneratorArguments<AtomGenerationTargetConfig>(
                    new AtomGenerationTargetConfig
                    {
                        Host = ".",
                        Database = "AcoDomain"
                    },
                    atomsFolder: null,
                    defaults: null));

            foreach (var item in results)
            {
                Console.WriteLine(item.FileName);
                Console.WriteLine("=====");
                Console.WriteLine(item.Output);
            }
        }
    }

    public class TestBase
    {
        public TestBase()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .CreateLogger();
        }
    }

    public class Tests : TestBase
    {
        [Fact]
        public void CreateSqlGenerates()
        {
            var atoms = AtomCreator.FromFolder("TestAtoms/CareRelated");

            foreach (var atom in atoms)
            {
                var str = SqlGenerator.CreateTable(atom);

                Console.WriteLine("=============");
                Console.WriteLine(atom.Name);
                Console.WriteLine("---");
                Console.WriteLine(str);
            }
        }

        [Fact]
        public void RunTestAtom()
        {
            var atom = AtomCreator.FromString(File.ReadAllText("TestAtoms/test.atom"));
            var str = SqlGenerator.CreateTable(atom);

            Console.WriteLine("=============");
            Console.WriteLine(atom.Name);
            Console.WriteLine("---");
            Console.WriteLine(str);

        }

        [Fact]
        public void InsertSprocGenerators()
        {
            var atom = AtomCreator.FromString(File.ReadAllText("Config.atom"));

            var str = new InsertSproc(atom).Generate();

            Console.WriteLine(str.Sql);
        }

        [Fact]
        public void UpdateSprocGenerators()
        {
            var atom = AtomCreator.FromString(File.ReadAllText("Config.atom"));

            var str = new UpdateSproc(atom).Generate();

            Console.WriteLine(str.Sql);
        }

        [Fact]
        public void ListSprocGenerators()
        {
            var atom = AtomCreator.FromString(File.ReadAllText("Config.atom"));

            var str = new GetOneSproc(atom).Generate();

            Console.WriteLine(str.Sql);
        }

        [Fact]
        public void BatchListSprocGenerators()
        {
            var atom = AtomCreator.FromString(File.ReadAllText("Config.atom"));

            var str = new BatchListSproc(atom).Generate();

            Console.WriteLine(str.Sql);
        }

        [Fact]
        public void TestTableTypeGeneration()
        {
            var generator = new TableTypeGenerator();

            foreach (var table in generator.GetCustomTableTypes())
            {
                Console.WriteLine(table.Sql);

                Console.WriteLine();
                Console.WriteLine("===");
                Console.WriteLine();
            }
        }

        [Fact(Skip = "I dont haz the folder")]
        public void WriteAll()
        {
            var path = @"C:\source\temp\db\sql\HackDay\RedGateScripts";

            var atom = AtomCreator.FromString(File.ReadAllText("Config.atom"));

            var table = SqlGenerator.CreateTable(atom);

            var tablePath = Path.Combine(path, "Tables", string.Format("dbo.{0}.sql", atom.Name));

            File.WriteAllText(tablePath, table.TableSql);

            foreach (var item in SqlGenerator.MakeSprocs(atom, new List<AtomModel>{ atom }))
            {
                var sprocs = Path.Combine(path, "Stored Procedures", string.Format("dbo.{0}.sql", item.Name));

                File.WriteAllText(sprocs, item.Sql);
            }

            foreach (var item in SqlGenerator.MakeCustomTables())
            {
                var sprocs = Path.Combine(path, "Types", "User-defined Data Types", string.Format("dbo.{0}Type.sql", item.Name));

                File.WriteAllText(sprocs, item.Sql);
            }
        }

        [Fact]
        public void ViewGenCarePlanComplex()
        {
            var allAtoms = AtomCreator.FromFolder("TestAtoms/CareRelated/");

            var projection = JsonConvert.DeserializeObject<QueryProjectionAtom>(File.ReadAllText("TestAtoms/CareRelated/CarePlan.proj.atom"));

            ViewGenerator generator = new ViewGenerator(projection, allAtoms: allAtoms);

            var refs = generator.Generate();

            Console.WriteLine(string.Join(Environment.NewLine, refs.References));
            Console.WriteLine(refs.Sql);
        }

        [Fact]
        public void ViewGenTestsTransitive()
        {
            var roots = new List<AtomModel>
                        {
                            new AtomModel
                            {
                                Name = "CareOrgManager",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareProgramGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareProgram"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareOrg",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgGuid"
                                              },
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgManagerGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareOrgManager"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareProgram",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareOrg"
                                                              }
                                              }
                                          }
                            },
                        };

            var gen = new QueryPlanBuilder(
                new ProjectionAtom(
                    new Dictionary<string, List<string>>
                    {
                        {
                            "CareOrgManager", new List<string>
                                              {
                                                  "CareOrgManagerGuid",
                                                  "B"
                                              }
                        },
                        { "CareProgram", new List<string>() },
                    }),
                roots.ToDictionary(a => a.Name));

            var result = gen.GetReferences();

            Console.WriteLine(string.Join("\r\n", result));
        }

        [Fact]
        public void ViewGenTests()
        {
            var roots = new List<AtomModel>
                        {
                            new AtomModel
                            {
                                Name = "CareOrgManager",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareProgramGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareProgram"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareOrg",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgGuid"
                                              },
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgManagerGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareOrgManager"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareProgram",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareOrg"
                                                              }
                                              }
                                          }
                            },
                        };

            QueryPlanBuilder gen = new QueryPlanBuilder(
                new ProjectionAtom(
                    new Dictionary<string, List<string>>
                    {
                        {
                            "CareOrg", new List<string>
                                       {
                                           "A",
                                           "CareOrgManagerGuid"
                                       }
                        },
                        {
                            "CareOrgManager", new List<string>
                                              {
                                                  "CareOrgManagerGuid",
                                                  "B"
                                              }
                        },
                        { "CareProgram", new List<string>() },
                    }),
                roots.ToDictionary(a => a.Name));

            var result = gen.GetReferences();

            Console.WriteLine(string.Join("\r\n", result));
        }

        [Fact]
        public void ViewGenWeird()
        {
            var roots = new List<AtomModel>
                        {
                            new AtomModel
                            {
                                Name = "CareOrgManager",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareProgramGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareProgram"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareOrg",
                                Members = new OrderedAtomMembers
                                          {
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgGuid"
                                              },
                                              new AtomMemberInfo
                                              {
                                                  Name = "CareOrgManagerGuid",
                                                  Reference = new AtomReference
                                                              {
                                                                  Name = "CareOrgManager"
                                                              }
                                              }
                                          }
                            },
                            new AtomModel
                            {
                                Name = "CareProgram",
                                Members = new OrderedAtomMembers()
                            },
                        };

            QueryPlanBuilder gen = new QueryPlanBuilder(
                new ProjectionAtom(
                    new Dictionary<string, List<string>>
                    {
                        {
                            "CareOrg", new List<string>
                                       {
                                           "A",
                                           "CareOrgManagerGuid"
                                       }
                        },
                        {
                            "CareOrgManager", new List<string>
                                              {
                                                  "CareOrgManagerGuid",
                                                  "B"
                                              }
                        },
                        { "CareProgram", new List<string>() },
                    }),
                roots.ToDictionary(a => a.Name));

            var result = gen.GetReferences();

            Console.WriteLine(string.Join("\r\n", result));
        }
    }
}