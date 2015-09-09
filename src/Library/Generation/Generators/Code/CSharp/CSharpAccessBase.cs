using System;
using System.Linq;
using Atom.Data;

namespace Atom.Generation.Generators.Code.CSharp
{
    public abstract class CSharpAccessBase 
    {
        protected CSharpTargetConfig Config { get; private set; }

        public CSharpAccessBase(CSharpTargetConfig config)
        {            
            Config = config;

            if (Config == null)
            {
                Config = new CSharpTargetConfig();
            }
        }

        protected string GetClassName(RepositoryMemberInfo repositoryMembmerInfo)
        {
            return CSharpCodeClassGenerator.GetClassName(repositoryMembmerInfo.Info.BaseAtom);
        }

        protected string GetMemberType(AtomMemberInfo member)
        {
            if (CanBeStronger(member))
            {
                var referencingTypeName = new CSharpStrongTypeNameFinder(member).TypeName();

                if (member.Optional)
                {
                    return referencingTypeName + "?";
                }

                return referencingTypeName;
            }

            string typeName = member.MemberType.Accept(new CSharpDefaultTypeFinder(member.Optional));

            return typeName;
        }

        protected bool CanBeStronger(AtomMemberInfo member)
        {
            if (!Config.Entities.StrongTypes || member.HasFlag(MemberFlags.Hidden))
            {
                return false;
            }

            return member.IsAltKey || member.IsPrimary || (member.HasReference &&
                    (member.Reference.IsReferenceToHiddenPrimaryKey || CanBeStronger(member.Reference.TargetMember)));
        }

        protected AtomMemberInfo GetStrongTypeMember(AtomMemberInfo member)
        {
            if (member.HasReference && member.Reference.IsReferenceToHiddenPrimaryKey)
            {
                return member.Reference.TargetAtomAlternateKey;
            }

            return member;
        }

        protected AtomMemberInfo FindMainKey(ProjectedAtomRoot returnType)
        {
            if (returnType.BasedOn == null)
            {
                throw new Exception("Cannot generate sproc on non atom (is this a projection?)");
            }

            if (returnType.BasedOn.AdditionalInfo.ShouldHidePrimaryKey() && !returnType.BasedOn.IsLookup)
            {
                return returnType.BasedOn.Members.FirstOrDefault(i => i.IsAltKey);
            }

            return returnType.BasedOn.Members.FirstOrDefault(i => i.IsPrimary);
        }

    }
}
