using System;
using System.Linq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;

namespace Atom.Generation.Generators.Code.CSharp
{
    internal class CSharpSprocAccessGenerator : CSharpAccessBase
    {
        public CSharpSprocAccessGenerator(CSharpTargetConfig config)
            : base(config)
        {
        }

        public CSharpClassResult GetClass(IGrouping<string, RepositoryMemberInfo> repoGroup)
        {
            var rawMembers = repoGroup.Select(GenerateMember)
                                      .ToList();

            var functionImplementations = string.Join(Environment.NewLine + Environment.NewLine, rawMembers.Select(i => i.Body))
                                                .Trim();

            var interfaceMembmers = string.Join(Environment.NewLine + Environment.NewLine, rawMembers.Select(i => i.Signature + ";"))
                                          .Trim();

            return GetClass(
                repoName: repoGroup.Key,
                interfaceMembers: interfaceMembmers.IndentAllLines(byTabs: 1, ignoreFirst: true),
                constructorBody: GetConstructorBody(),
                members: functionImplementations.IndentAllLines(byTabs: 1, ignoreFirst: true));

        }

        public CSharpClassResult GetClass(string repoName, string interfaceMembers, string constructorBody, string members)
        {
            string classTemplate = $@"public partial interface I{repoName}Repository
{{
    {interfaceMembers}
}}

public partial class {repoName}Repository : BaseRepository<{repoName}Repository>, I{repoName}Repository
{{
    public {repoName}Repository(string connectionString)
        : base(connectionString)
    {{
        {constructorBody}
    }}

    {members}
}}";

            string classCode = classTemplate;

            return new CSharpClassResult
            {
                ClassCode = classCode,
                Name = repoName + "Repository"
            };
        }

        public RepositoryResult Generate(IGrouping<string, RepositoryMemberInfo> repoGroup)
        {
            var template = $@"
{new NamespaceGenerator().ByConfig(Config)}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PF.Contrib.Dapper.Extensions;
using {Config.Entities.Namespace};

namespace {Config.Repository.Namespace}
{{
    {GetClass(repoGroup).ClassCode}
}}".Trim();

            return new RepositoryResult
            {
                CodeString = template,
                Name = repoGroup.Key + "Repository.cs"
            };
        }

        private string GetConstructorBody()
        {
            if (Config.Entities.StrongTypes)
            {
                return "Database.InitTypeMappings();";
            }

            return string.Empty;
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateMember(RepositoryMemberInfo arg)
        {
            switch (arg.Info.QueryType)
            {
                case QueryType.Insert:
                    return GenerateInsert(arg);
                case QueryType.Upsert:
                    return GenerateUpsert(arg);
                case QueryType.Update:
                    return GenerateUpdate(arg);
                case QueryType.Delete:
                    return GenerateSoftDelete(arg);
                case QueryType.GetOne:
                    return GenerateList(arg);
                case QueryType.GetBy:
                    return GenerateListByForeignKey(arg);
                case QueryType.GetAll:
                    return GenerateGetAll(arg);
                case QueryType.BatchList:
                    return GenerateBatchList(arg);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateGetAll(RepositoryMemberInfo repositoryMembmerInfo)
        {
            string verb = "GetAll";
            string returnType = GetClassName(repositoryMembmerInfo);

            var signature = $"Task<List<{returnType}>> {verb}()";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var result = await db.QuerySprocAsync<{returnType}>(""{repositoryMembmerInfo.Info.Name}"").ConfigureAwait(continueOnCapturedContext: false);

        return result.ToList();
    }}
}}";
            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateBatchList(RepositoryMemberInfo repositoryMembmerInfo)
        {
            string returnType = GetClassName(repositoryMembmerInfo);
            var mainKey = repositoryMembmerInfo.Info.QueryKey;

            string mainKeyType = GetMemberType(mainKey);

            string mainKeyRealType = new CSharpDefaultTypeFinder(mainKey.Optional).Visit(mainKey.MemberType);

            string casting = Config.Entities.StrongTypes ? $"Select(id => ({mainKeyRealType})id).ToList()." : string.Empty;

            var signature = $"Task<List<{returnType}>> GetMany(IEnumerable<{mainKeyType}> ids)";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            LookupKeys = ids.{casting}ToDataTable()
                        }};

        var result = await db.QuerySprocAsync<{returnType}>(""{repositoryMembmerInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false);

        return result.ToList();
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateList(RepositoryMemberInfo repositoryMembmerInfo)
        {
            if (repositoryMembmerInfo.Info.QueryKey?.HasReference ?? false)
            {
                return GenerateListByForeignKey(repositoryMembmerInfo);
            }

            return GenerateListByPrimaryKey(repositoryMembmerInfo);
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateListByForeignKey(RepositoryMemberInfo repositoryMembmerInfo)
        {
            string returnType = GetClassName(repositoryMembmerInfo);
            var mainKey = repositoryMembmerInfo.Info.QueryKey;

            string mainKeyType = GetMemberType(mainKey);
            string methodSuffix = mainKey.HasReference && (!Config.Entities.StrongTypes || !CanBeStronger(mainKey)) ? mainKey.Name : "";
            string mainKeyFieldLower = StringExt.ToCamelCase(mainKey.Name);

            var signature = $"Task<List<{returnType}>> GetBy{methodSuffix}({mainKeyType} {mainKeyFieldLower})";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {mainKey.Name} = {StrongTypeCastingType(mainKey)}{mainKeyFieldLower}
                        }};

        var result = await db.QuerySprocAsync<{returnType}>(""{repositoryMembmerInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false);

        return result.ToList();
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateListByPrimaryKey(RepositoryMemberInfo repositoryMemberInfo)
        {
            var mainKey = repositoryMemberInfo.Info.QueryKey;
            var MethodSuffix = mainKey.HasReference && (!Config.Entities.StrongTypes || !CanBeStronger(mainKey)) ? "By" + mainKey.Name : "";
            string returnType = GetClassName(repositoryMemberInfo);
            string mainKeyName = mainKey.Name;
            string mainKeyType = GetMemberType(mainKey);
            string mainKeyFieldLower = StringExt.ToCamelCase(mainKey.Name);
            string sprocName = repositoryMemberInfo.Info.Name;
            string strongTypeCasting = StrongTypeCastingType(mainKey);



            var signature = $"Task<{returnType}> GetOne{MethodSuffix}({mainKeyType} {mainKeyFieldLower})";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {mainKeyName} = {strongTypeCasting}{mainKeyFieldLower}
                        }};

        var result = await db.QuerySprocAsync<{returnType}>(""{sprocName}"", payload).ConfigureAwait(continueOnCapturedContext: false);

        return result.FirstOrDefault();
    }}
}}";
            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private string StrongTypeCastingType(AtomMemberInfo member)
        {
            if (Config.Entities.StrongTypes &&
                CanBeStronger(member))
            {
                var type = member.MemberType;
                if (member.HasReference && member.Reference.IsReferenceToHiddenPrimaryKey)
                {
                    type = member.Reference.TargetAtomAlternateKey.MemberType;
                }
                return "(" + new CSharpDefaultTypeFinder(member.Optional).Visit(type) + ")";
            }

            return string.Empty;
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateUpdate(RepositoryMemberInfo repositoryMembmerInfo)
        {
            var className = CSharpCodeClassGenerator.GetClassName(repositoryMembmerInfo.Info.BaseAtom);
            string updateFields = GetUpdateFields(repositoryMembmerInfo, StringExt.ToCamelCase(className))
                               .IndentAllLines(7, true);

            var signature = $"Task Update({className} {StringExt.ToCamelCase(className)})";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {updateFields}
                        }};

        await db.ExecSprocAsync(""{repositoryMembmerInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false);
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateSoftDelete(RepositoryMemberInfo repositoryMembmerInfo)
        {
            var mainKey = repositoryMembmerInfo.Info.QueryKey;
            string mainKeyName = mainKey.Name;
            string mainKeyType = GetMemberType(mainKey);
            string mainKeyFieldLower = StringExt.ToCamelCase(mainKey.Name);
            string strongTypeCasting = StrongTypeCastingType(mainKey);


            var signature = $"Task SoftDelete({mainKeyType} {mainKeyFieldLower})";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {mainKeyName} = {strongTypeCasting}{mainKeyFieldLower}
                        }};

        await db.ExecSprocAsync(""{repositoryMembmerInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false);
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }


        private string GetUpdateFields(RepositoryMemberInfo repositoryMembmerInfo, string className)
        {

            var queryParams = repositoryMembmerInfo.Info.InputParams.Select(
                memberInfo =>
                {
                    var parameterName = memberInfo.Name;
                    if (memberInfo.HasReference && memberInfo.Reference.IsReferenceToHiddenPrimaryKey)
                    {
                        parameterName = memberInfo.Reference.TargetAtomAlternateKey.Name;
                    }
                    string memberName = new CSharpMemberNameFinder(memberInfo).MemberName();
                    return $"{parameterName} = {StrongTypeCastingType(memberInfo)}({className}.{memberName})";
                });

            return string.Join("," + Environment.NewLine, queryParams)
                         .Trim();
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateInsert(RepositoryMemberInfo repositoryMemberInfo)
        {

            var className = GetClassName(repositoryMemberInfo);
            string typeName = StringExt.ToCamelCase(className);

            var mainKey = FindMainKey(repositoryMemberInfo.Info.BaseAtom);
            if (mainKey == null)
            {
                throw new Exception(String.Format("{0} has no main key", repositoryMemberInfo.BaseAtomTypeName));
            }

            string keyType = GetMemberType(mainKey);

            string updateFields = GetUpdateFields(repositoryMemberInfo, StringExt.ToCamelCase(className))
                               .IndentAllLines(7, true);

            string signature;
            string finalStatement;

            if (repositoryMemberInfo.Info.BaseAtom.BasedOn.AdditionalInfo.SelectAfterInsert.GetValueOrDefault(true))
            {
                signature = $"Task<{keyType}> Insert({className} {typeName})";
                finalStatement = $@"return (await db.QuerySprocAsync<{keyType}>(""{repositoryMemberInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault();";
            }
            else
            {
                signature = $"Task Insert({className} {typeName})";
                finalStatement = $@"await db.ExecSprocAsync(""{repositoryMemberInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false);";
            }
            
            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {updateFields}
                        }};

        {finalStatement}
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }

        private CSharpRepoGenerator.RepositoryFunction GenerateUpsert(RepositoryMemberInfo repositoryMembmerInfo)
        {

            var className = GetClassName(repositoryMembmerInfo);
            string typeName = StringExt.ToCamelCase(className);

            var mainKey = FindMainKey(repositoryMembmerInfo.Info.BaseAtom);
            string keyType = GetMemberType(mainKey);

            string updateFields = GetUpdateFields(repositoryMembmerInfo, StringExt.ToCamelCase(className))
                               .IndentAllLines(7, true);

            var signature = $"Task<{keyType}> Upsert({className} {typeName})";

            var template = $@"
public async {signature}
{{
    using (var db = GetDb())
    {{
        var payload = new
                        {{
                            {updateFields}
                        }};

        return (await db.QuerySprocAsync<{keyType}>(""{repositoryMembmerInfo.Info.Name}"", payload).ConfigureAwait(continueOnCapturedContext: false)).FirstOrDefault();
    }}
}}";

            return new CSharpRepoGenerator.RepositoryFunction
            {
                Body = template,
                Signature = signature
            };
        }
    }
}
