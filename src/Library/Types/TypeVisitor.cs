using System;
using Atom.Data.Types;
using Atom.Generation.Generators.Code.CSharp;

namespace Atom.Types
{
    public class TypeVisitor<TResult> : StrictTypeVisitor<TResult>
    {
        public virtual TResult DefaultVisit(MemberType type)
        {
            return default(TResult);
        }

        public override TResult Visit(MemberGuid memberGuid)
        {
            return DefaultVisit(memberGuid);
        }

        public override TResult Visit(MemberText memberText)
        {
            return DefaultVisit(memberText);
        }

        public override TResult Visit(MemberFloat memberText)
        {
            return DefaultVisit(memberText);
        }

        public override TResult Visit(MemberByte memberByte)
        {
            return DefaultVisit(memberByte);
        }

        public override TResult Visit(MemberDouble memberDouble)
        {
            return DefaultVisit(memberDouble);
        }

        public override TResult Visit(MemberShort memberShort)
        {
            return DefaultVisit(memberShort);
        }

        public override TResult Visit(MemberDecimal memberDecimal)
        {
            return DefaultVisit(memberDecimal);
        }

        public override TResult Visit(MemberBinary memberBinary)
        {
            return DefaultVisit(memberBinary);
        }

        public override TResult Visit(MemberLong memberLong)
        {
            return DefaultVisit(memberLong);
        }

        public override TResult Visit(MemberDateTime memberDateTime)
        {
            return DefaultVisit(memberDateTime);
        }

        public override TResult Visit(MemberDate memberDate)
        {
            return DefaultVisit(memberDate);
        }

        public override TResult Visit(MemberBool memberBool)
        {
            return DefaultVisit(memberBool);
        }
    }
}
