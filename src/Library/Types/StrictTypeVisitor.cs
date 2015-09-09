using System;
using Atom.Data.Types;
using Atom.Generation.Generators.Code;

namespace Atom.Types
{
    public abstract class StrictTypeVisitor<TResult>: ITypeVisitor<TResult>
    {
        public virtual TResult Visit(MemberType symbol)
        {
            if (symbol == null)
            {
                return default(TResult);
            }
            else
            {
                return symbol.Accept<TResult>(this);
            }
        }

        public abstract TResult Visit(MemberGuid memberGuid);

        public abstract TResult Visit(MemberText memberText);

        public abstract TResult Visit(MemberFloat memberText);

        public abstract TResult Visit(MemberDecimal memberDecimal);

        public abstract TResult Visit(MemberByte memberByte);

        public abstract TResult Visit(MemberShort memberShort);

        public abstract TResult Visit(MemberDouble memberDouble);

        public abstract TResult Visit(MemberBinary memberText);

        public abstract TResult Visit(MemberLong memberLong);

        public abstract TResult Visit(MemberDateTime memberDateTime);

        public abstract TResult Visit(MemberDate memberDate);

        public abstract TResult Visit(MemberBool memberBool);
    }
}
