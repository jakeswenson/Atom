using Atom.Data.Types;
using Atom.Generation.Generators.Code;
using Atom.Types;
using System.Text.RegularExpressions;

namespace Atom.Generation.Generators.Sql.Types
{
    public class SqlDescriptionVisitor : TypeVisitor<string>
    {
        private readonly SqlTypeNameVistor _typeNameVistor = new SqlTypeNameVistor();

        public override string DefaultVisit(MemberType type)
        {
            // If a type contains a precision value (e.g., datetime2(0)), the brackets must only be around the name part
            return Regex.Replace(type.Accept(_typeNameVistor), "^[A-Za-z0-9]+", match => "[" + match.Value + "]");
        }

        public override string Visit(MemberBinary memberBinary)
        {
            return _typeNameVistor.Visit(memberBinary);
        }

        public override string Visit(MemberText memberText)
        {
            return _typeNameVistor.Visit(memberText) + " COLLATE SQL_Latin1_General_CP1_CI_AS";
        }
    }
}
