using System;
using Atom.Data.Types;
using Atom.Types;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpDefaultTypeFinder : StrictTypeVisitor<string>
    {
        private readonly bool _optional;

        public CSharpDefaultTypeFinder(bool optional)
        {
            _optional = optional;
        }

        public override string Visit(MemberBool memberBool)
        {
            return "bool" + Optionality();
        }

        private string Optionality()
        {
            if (_optional)
            {
                return "?";
            }

            return string.Empty;
        }

        public override string Visit(MemberDate memberDate)
        {
            return "System.DateTime" + Optionality();
        }

        public override string Visit(MemberGuid memberGuid)
        {
            return "System.Guid" + Optionality();
        }

        public override string Visit(MemberBinary member)
        {
            return "byte[]";
        }

        public override string Visit(MemberFloat member)
        {
            return "float" + Optionality();
        }

        public override string Visit(MemberLong memberLong)
        {
            return "long" + Optionality();
        }

        public override string Visit(MemberDateTime memberDateTime)
        {
            return "System.DateTime" + Optionality();
        }

        public override string Visit(MemberText memberText)
        {
            return "string";
        }

        public override string Visit(MemberDecimal memberDecimal)
        {
            return "decimal" + Optionality();
        }

        public override string Visit(MemberByte memberByte)
        {
            return "byte" + Optionality();
        }

        public override string Visit(MemberShort memberShort)
        {
            return "short" + Optionality();
        }

        public override string Visit(MemberDouble memberDouble)
        {
            return "double" + Optionality();
        }
    }
}
