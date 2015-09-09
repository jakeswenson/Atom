using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberGuid : MemberType
    {
        public override string ValidateDefault(string value)
        {
            if (value.Equals("newid()", StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            var g = new Guid();
            if (Guid.TryParse(value, out g))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid guid default");
        }

        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                return true;
            }
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}
