using Atom.Data.Types;
using Atom.Types;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpTypeOptionalityFinder : TypeVisitor<bool>
    {
        public override bool Visit(MemberBool memberBool)
        {
            return true;
        }

        public override bool Visit(MemberDate memberDate)
        {
            return true;
        }

        public override bool Visit(MemberDateTime memberDateTime)
        {
            return true;
        }

        public override bool Visit(MemberFloat memberText)
        {
            return true;
        }

        public override bool Visit(MemberGuid memberGuid)
        {
            return true;
        }

        public override bool Visit(MemberLong memberLong)
        {
            return true;
        }

        public override bool Visit(MemberDecimal memberDecimal)
        {
            return true;
        }

        public override bool Visit(MemberByte memberByte)
        {
            return true;
        }

        public override bool Visit(MemberDouble memberDouble)
        {
            return true;
        }

        public override bool Visit(MemberShort memberShort)
        {
            return true;
        }

        public override bool DefaultVisit(MemberType type)
        {
            return false;
        }
    }
}