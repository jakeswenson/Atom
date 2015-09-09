using System;
using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpSingleRepositoryGenerator
    {
        private readonly CSharpTargetConfig _config;

        public CSharpSingleRepositoryGenerator(CSharpTargetConfig config)
        {
            if (config == null)
            {
                config = new CSharpTargetConfig();
            }

            _config = config;
        }

        internal RepositoryResult Generate(List<RepositoryMemberInfo> atoms)
        {
            SingleRepositoryResult singleRepositoryResult = GenerateSingleRepo(atoms);
            var namespaces = new NamespaceGenerator();
            string usings = string.Join(Environment.NewLine, namespaces.Usings(singleRepositoryResult.Namespaces));

            string repos = string.Join(Environment.NewLine + Environment.NewLine, singleRepositoryResult.Classes
                                             .Select(c => c.ClassCode.IndentAllLines(byTabs: 1)));

            string singleRepoTemplate = $@"
{singleRepositoryResult.Header}
{usings}

namespace {singleRepositoryResult.RepositoryNamespace}
{{
{repos}

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

    public partial interface IReadableRepository<TRecord, TKey>
    {{
        Task<TRecord> GetOne(TKey key);

        Task<IEnumerable<TRecord>> GetAll(); 
    }}

    public partial interface IWritableRepository<TRecord, TKey>
    {{
        Task<TKey> Insert(TRecord record);

        Task Update(TRecord record);
    }}

    public partial interface IRepository<TRecord, TKey> : IReadableRepository<TRecord, TKey>, IWritableRepository<TRecord, TKey>
    {{
    }}
}}
".Trim();

            return new RepositoryResult
            {
                Name = "Repositories.cs",
                CodeString = singleRepoTemplate
            };
        }

        private SingleRepositoryResult GenerateSingleRepo(IEnumerable<RepositoryMemberInfo> atoms)
        {
            var namespaceGenerator = new NamespaceGenerator();

            var result = new SingleRepositoryResult
            {
                Header = namespaceGenerator.Header(),
                Namespaces = namespaceGenerator.GetNamespaces(_config).Concat(new[] {
                    "PF.Common.SqlClient",
                    _config.Entities.Namespace,
                    "PF.Contrib.Dapper.Extensions"
                }).ToList(),
                Classes = GenerateSprocs(atoms).ToList(),
                RepositoryNamespace = _config.Repository.Namespace
            };

            return result;
        }

        public RepositoryResult GenerateDbFile(List<RepositoryMemberInfo> atoms)
        {
            var key = new KeyValuePair<string, List<RepositoryMemberInfo>>("Database", atoms);

            var dbFile = new CSharpDapperDbAccessGenerator(_config).Generate(key);

            return dbFile;
        }

        private IEnumerable<CSharpClassResult> GenerateSprocs(IEnumerable<RepositoryMemberInfo> atoms)
        {
            var groups = atoms.Where(i => i.Info.QueryType != QueryType.View)
                              .GroupBy(i => i.BaseAtomTypeName);

            return groups.Select(new CSharpSprocAccessGenerator(_config).GetClass);
        }

        internal class RepositoryFunction
        {
            public string Signature { get; set; }

            public string Body { get; set; }
        }
    }
}
