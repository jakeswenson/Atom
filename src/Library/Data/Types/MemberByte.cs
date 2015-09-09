using System;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberByte : MemberType
    {
        public override string ValidateDefault(string value)
        {
            byte parsedValue;
            if (byte.TryParse(value, out parsedValue))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid byte default");
        }

        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                return false;
            }
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}