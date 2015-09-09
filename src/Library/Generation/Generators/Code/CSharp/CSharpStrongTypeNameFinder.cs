using System.Linq;
using Atom.Data;
using Atom.Generation.Extensions;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpStrongTypeNameFinder
    {
        private readonly AtomMemberInfo _member;

        public CSharpStrongTypeNameFinder(AtomMemberInfo member)
        {
            _member = member;
        }

        public string TypeName()
        {
            if (_member.HasReference)
            {
                if (_member.Reference.TargetMember.Atom.IsLookup)
                {
                    return StringExt.ToTitleCase(_member.Reference.TargetMember.Atom.Name);
                }

                if (_member.Reference.IsReferenceToHiddenPrimaryKey)
                {
                    return
                        new CSharpStrongTypeNameFinder(_member.Reference.TargetAtomAlternateKey)
                            .TypeName();
                }

                return new CSharpStrongTypeNameFinder(_member.Reference.TargetMember).TypeName();
            }

            return StringExt.ToTitleCase(_member.Name);
        }
    }
}
