using System;
using Atom.Data.Types;
using Atom.Generation.Generators.Code;
using Atom.Generation.Generators.Code.CSharp;
using Atom.Types;

namespace Atom.Generation.Generators.Sql.Types
{
    public class SqlTypeNameVistor : StrictTypeVisitor<string>
    {
        public override string Visit(MemberBinary memberBinary)
        {
            string typeName = !memberBinary.VariableSize ? "binary" : "varbinary";
            return typeName + "(" + (memberBinary.Length == -1 ? "MAX" : memberBinary.Length.ToString()) + ")";
        }

        public override string Visit(MemberGuid memberGuid)
        {
            return "uniqueidentifier";
        }

        public override string Visit(MemberBool memberBool)
        {
            return "bit";
        }

        public override string Visit(MemberDecimal memberDecimal)
        {
            return "decimal";
        }

        public override string Visit(MemberShort memberShort)
        {
            return "smallint";
        }

        public override string Visit(MemberDouble memberDouble)
        {
            return "float";
        }

        public override string Visit(MemberByte memberByte)
        {
            return "tinyint";
        }

        public override string Visit(MemberFloat memberText)
        {
            return "single";
        }

        public override string Visit(MemberLong memberLong)
        {
            return "bigint";
        }

        public override string Visit(MemberText memberText)
        {
            string typeName = memberText.IsUnicode ? "nvarchar" : "varchar";

            string lengthString = memberText.Length == -1 ? "MAX" : memberText.Length.ToString();

            return typeName + "(" + lengthString + ")";

        }

        public override string Visit(MemberDate memberDate)
        {
            return "date";
        }

        public override string Visit(MemberDateTime memberDateTime)
        {
            if (memberDateTime.UseDateTime2)
            {
                // 7 is the default number of decimal places of precision in a datetime2
                return memberDateTime.DateTime2Precision == 7 ? "datetime2" : $"datetime2({memberDateTime.DateTime2Precision})";
            }
            else
            {
                return "datetime";
            }
        }
    }
}
