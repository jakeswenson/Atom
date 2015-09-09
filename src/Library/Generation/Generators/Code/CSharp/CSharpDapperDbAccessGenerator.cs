using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Atom.Data;
using Atom.Generation.Data;
using Atom.Generation.Extensions;

namespace Atom.Generation.Generators.Code.CSharp
{
    internal class CSharpDapperDbAccessGenerator : CSharpAccessBase
    {        
        public CSharpDapperDbAccessGenerator(CSharpTargetConfig config) : base(config)
        {
        }

        public RepositoryResult Generate(KeyValuePair<string, List<RepositoryMemberInfo>> arg)
        {
            string ExtraNamespaces = new NamespaceGenerator().ByConfig(Config);
            string groupName = arg.Key;

            var classesToGenerate = arg.Value.DistinctBy(i => i.Info.BaseAtom.Name);

            var accessors = string.Join(Environment.NewLine + Environment.NewLine, classesToGenerate.Select(CreateAccessor))
                                  .Trim();

            string optionalStrongTypes = GenerateStrongTypeInitializers(arg.Key, arg.Value.First()).IndentAllLines(byTabs: 1, ignoreFirst: true);

            var template = $@"
{ExtraNamespaces}
using PF.Contrib.Dapper.Extensions;
using Dapper;
using System.Configuration;
using {Config.Entities.Namespace};

namespace {Config.Repository.Namespace}
{{  
    public partial class {groupName} : Database<{groupName}>
    {{        
        {accessors.IndentAllLines(byTabs: 2, ignoreFirst: true)}

        public static {groupName} Create(string connectionString){{
            return {groupName}.Init(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
        }}
    }}

    {optionalStrongTypes}
}}".Trim();

            return new RepositoryResult
                   {
                       Name = arg.Key + ".cs",
                       CodeString = template
                   };
        }

        private string GenerateStrongTypeInitializers(string groupName, RepositoryMemberInfo sample)
        {
            if (!Config.Entities.StrongTypes)
            {
                return string.Empty;
            }

            return $@"
public partial class {groupName}
{{
    public Database()
    {{
        InitTypeMappings();
    }}

    public static void InitTypeMappings()
    {{
        // get assembly where data records live

        var strongTypes = 
                Assembly.GetAssembly(typeof({GetClassName(sample)}))
                    .FindStrongTypes();

        foreach (var strongType in strongTypes)
        {{
            SqlMapper.AddTypeHandler(strongType, new StrongTypeMapper());
        }}
    }}
}}


internal class StrongTypeMapper : SqlMapper.ITypeHandler
{{
    public void SetValue(IDbDataParameter parameter, object value)
    {{
        parameter.DbType = new DbTypeStrongVisitor().Visit(value as IAcceptStrongTypeVisitor);

        parameter.Value = new UnderlyingStrongTypeVisitor().Visit(value as IAcceptStrongTypeVisitor);
    }}
    
    public object Parse(Type destinationType, object value)
    {{
        var targetType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;

        return Activator.CreateInstance(
            targetType,
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static,
            binder: null,
            args: new[] {{ value }},
            culture: null);
    }}
}}

internal class DbTypeStrongVisitor : IStrongTypeVistor<DbType>
{{
       
    public DbType Visit(ITypedGuid guid)
    {{
        return DbType.Guid;
    }}

    public DbType Visit(ITypedInt data)
    {{
        return DbType.Int32;
    }}

    public DbType Visit(ITypedLong data)
    {{
        return DbType.Int64;
    }}

    public DbType Visit(ITypedFloat data)
    {{
        return DbType.Double;
    }}

    public DbType Visit(ITypedDouble data)
    {{
        return DbType.Decimal;
    }}

    public DbType Visit(ITypedShort data)
    {{
        return DbType.Int16;
    }}

    public DbType Visit(ITypedByte data)
    {{
        return DbType.Byte;
    }}

    public DbType Visit(ITypedDateTime data)
    {{
        return DbType.DateTime;
    }}

    public DbType Visit(ITypedString data)
    {{
        return DbType.String;
    }}

    public DbType Visit(IAcceptStrongTypeVisitor visitor)
    {{
        return visitor.Accept(this);
    }}
}}";
        }

        private string CreateAccessor(RepositoryMemberInfo arg)
        {
            if (arg.Info.QueryType != QueryType.View)
            {
                return GenericAccessor(arg);
            }

            return ViewAccessor(arg);
        }

        private string ViewAccessor(RepositoryMemberInfo repositoryMemberInfo)
        {
            string returnType = GetClassName(repositoryMemberInfo);
            var template = $@"
public IReadOnlyTable<{returnType}> {repositoryMemberInfo.Info.BaseAtom.Name}
{{
    get {{ return CreateTable<{returnType}>(tableName: ""{repositoryMemberInfo.Info.Name}""); }}
}}";
            return template;
        }

        private string GenericAccessor(RepositoryMemberInfo repositoryMemberInfo)
        {
            string returnType = GetClassName(repositoryMemberInfo);
            var key = FindMainKey(repositoryMemberInfo.Info.BaseAtom);
            string keyType = GetMemberType(key);

            var template = $@"
public Table<{returnType}, {keyType}> {repositoryMemberInfo.Info.BaseAtom.Name}
{{
    get {{ return CreateTable<{returnType}, {keyType}>(i => i.{key.Name}); }}
}}";

            return template;
        }
    }
}
