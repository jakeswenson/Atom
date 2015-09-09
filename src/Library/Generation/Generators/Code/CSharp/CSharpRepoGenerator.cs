using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Atom.Data;
using Atom.Generation.Data;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpRepoGenerator
    {
        private readonly CSharpTargetConfig _config;

        public CSharpRepoGenerator(CSharpTargetConfig config)
        {
            if (config == null)
            {
                config = new CSharpTargetConfig();
            }

            _config = config;
        }

        internal IEnumerable<RepositoryResult> Generate(List<RepositoryMemberInfo> atoms)
        {
            var rawRepos = GenerateSprocs(atoms);

            var dapperDbs = GenerateDbFile(atoms);

            var baseRepo = GenerateBaseRepo();

            return baseRepo.Concat(rawRepos)
                           .Concat(dapperDbs);
        }

        private RepositoryResult GenerateBaseRepo()
        {
            var template = $@"
{new NamespaceGenerator().ByConfig(_config)}
using PF.Common.SqlClient;

namespace {_config.Repository.Namespace}
{{
    public abstract partial class BaseRepository<TRepo> : SqlClientRepositoryBase
    {{
        protected BaseRepository(string connectionString)
            : base(connectionString)
        {{                        
        }}
        
        protected Database GetDb()
        {{
            return Database.Init(OpenConnection(), TimeSpan.FromSeconds(30));
        }}

        public static TRepo FromConfig(string connectionString)
        {{
            return (TRepo) Activator.CreateInstance(typeof (TRepo), GetConnectionStringFromConfig(connectionString));
        }}
    }}    
}}
".Trim();
            return new RepositoryResult
            {
                CodeString = template,
                Name = "BaseRepository.cs"
            };
        }

        private IEnumerable<RepositoryResult> GenerateDbFile(List<RepositoryMemberInfo> atoms)
        {
            var key = new KeyValuePair<string, List<RepositoryMemberInfo>>("Database", atoms);

            var dbFile = new CSharpDapperDbAccessGenerator(_config).Generate(key);

            yield return dbFile;
        }

        private IEnumerable<RepositoryResult> GenerateSprocs(IEnumerable<RepositoryMemberInfo> atoms)
        {
            var groups = atoms.Where(i => i.Info.QueryType != QueryType.View)
                              .GroupBy(i => i.BaseAtomTypeName);

            return groups.Select(new CSharpSprocAccessGenerator(_config).Generate);
        }

        internal class RepositoryFunction
        {
            public string Signature { get; set; }

            public string Body { get; set; }
        }
    }
}
