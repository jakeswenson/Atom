using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Atom.Data;
using Atom.Data.Projections;
using Atom.Generation.Data;
using Atom.Generation.Extensions;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpCodeClassGenerator : CSharpAccessBase
    {
        public CSharpCodeClassGenerator(CSharpTargetConfig config)
            : base(config)
        {
        }

        public static string GetClassName(ProjectedAtomRoot atom)
        {
            if (atom.BasedOn != null && atom.BasedOn.AdditionalInfo.Schema != Constants.DefaultSchema)
            {
                return atom.Name + "Record";
            }

            return atom.Name + "Record";
        }

        public IEnumerable<GeneratorOutput> Generate(IEnumerable<ProjectedAtomRoot> atoms)
        {
            atoms = atoms.Where(a => a.BasedOn.AdditionalInfo.ShouldGenerateCode()).ToList();

            var classes = atoms.Select(GenerateClass)
                               .ToList();

            var strongTypesLookup = atoms.SelectMany(atom => atom.Members.Where(i => CanBeStronger(i.Member)).Select(j => AliasedAtomMemberInfo.FromAtomMemberInfo(GetStrongTypeMember(j.Member))))
                                    .ToLookup(t => new CSharpStrongTypeNameFinder(t.Member).TypeName());

            var strongTypes = strongTypesLookup.Select(c => StrongTypeDefinition(c.First()));


            string strongTypesCode = Config.Entities.StrongTypes ? string.Join(Environment.NewLine, strongTypes.IndentAllLines(1)) : string.Empty;

            yield return new GeneratorOutput(
                Path.ChangeExtension("Records.cs", Config.FileExtension),
                $@"
{new NamespaceGenerator().ByConfig(Config)}

namespace {Config.Entities.Namespace}
{{
    {string.Join(Environment.NewLine, classes.IndentAllLines(1)).Trim()}

    {strongTypesCode}
}}
".Trim());

        }



        private string GenerateClass(ProjectedAtomRoot atom)
        {
            return $@"
[Serializable]
public partial class {GetClassName(atom)}
{{
    {string.Join(Environment.NewLine, GetMembers(atom).IndentAllLines(1)).Trim()}
}}";
        }

        private IEnumerable<string> GetMembers(ProjectedAtomRoot atom)
        {
            var members = atom.Members.Where(mem => !mem.Member.HasFlag(MemberFlags.Hidden));

            foreach (var member in members)
            {
                string name = new CSharpMemberNameFinder(member).MemberName();
                string type = GetMemberType(member.Member);

                if (member.Member.HasFlag(MemberFlags.TemporalTracking))
                {
                    yield return $"public {type} {name} {{ get; private set; }}";
                }
                else
                {
                    yield return $"public {type} {name} {{ get; set; }}";
                }

                if (member.Member.HasReference &&
                    member.Member.Reference.TargetMember.Atom.IsLookup)
                {
                    yield return $"private {type} {name}Id {{ set {{ {name} = value; }} }}";
                }
            }
        }

        private string StrongTypeDefinition(AliasedAtomMemberInfo arg)
        {
            var text = arg.Member.MemberType.Accept(new CSharpStrongTypeImplementationFinder(arg.Member));

            if (text != null)
            {
                return text;
            }
            throw new Exception($"Strong type for field {arg.Name} of type {arg.Member.MemberType} isn't available");
        }
    }
}
