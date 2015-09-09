using System.Collections.Generic;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;
using Atom.Generation.Generators.Sql.Types;

namespace Atom.Generation.Generators.Sql.Tables
{
    public class AtomMemberDefinitions
    {
        private readonly AtomModel _atomModel;

        private bool _primaryKeySet;

        public AtomMemberDefinitions(AtomModel atomModel)
        {
            _atomModel = atomModel;
        }

        public IEnumerable<string> GetDefinitions()
        {
            return _atomModel.Members.Select(Generate);
        }

        private string Generate(AtomMemberInfo member)
        {
            var type = GetMemberType(member);

            var constraint = GetMemberConstraint(member);

            var defaultValue = GetDefaultValue(member.Name, member);

            return $@"[{member.Name}] {type} {constraint} {defaultValue}".Trim();
        }

        private string GetDefaultValue(string name, AtomMemberInfo value)
        {
            // prevent duplicate primary keys getting the Identity Value

            if (!_primaryKeySet &&
                value.IsPrimary &&
                value.MemberType is MemberLong &&
                !_atomModel.IsLookup)
            {
                _primaryKeySet = true;

                return "IDENTITY(1,1)";
            }

            if (!string.IsNullOrEmpty(value.DefaultValue))
            {
                return $"CONSTRAINT [DF_{_atomModel.Name}_{name}] DEFAULT ({ValidateDefault(value)})";
            }

            return string.Empty;
        }

        private string ValidateDefault(AtomMemberInfo value)
        {
            return value.MemberType.ValidateDefault(value.DefaultValue);
        }

        private string GetMemberConstraint(AtomMemberInfo value)
        {
            if (value.IsPrimary || 
                value.IsAltKey ||
                !value.Optional)
            {
                return "NOT NULL";
            }

            return "NULL";
        }

        private string GetMemberType(AtomMemberInfo value)
        {
            return value.MemberType.Accept(new SqlDescriptionVisitor());
        }
    }
}
