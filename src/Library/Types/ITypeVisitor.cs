using Atom.Data.Types;

namespace Atom.Types
{
    public interface ITypeVisitor<out TResult>
    {
        TResult Visit(MemberType type);

        TResult Visit(MemberGuid memberGuid);

        TResult Visit(MemberText memberText);

        TResult Visit(MemberFloat memberFloat);

        TResult Visit(MemberDouble memberDouble);

        TResult Visit(MemberDecimal memberDecimal);

        TResult Visit(MemberShort memberShort);

        TResult Visit(MemberByte memberByte);

        TResult Visit(MemberBinary memberBinary);

        TResult Visit(MemberLong memberLong);

        TResult Visit(MemberDateTime memberDateTime);

        TResult Visit(MemberDate memberDate);

        TResult Visit(MemberBool memberBool);
    }
}
