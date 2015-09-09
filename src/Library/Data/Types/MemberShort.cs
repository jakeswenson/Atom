using System;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberShort : MemberType
    {
        public override string ValidateDefault(string value)
        {
            short parsedValue;
            if (short.TryParse(value, out parsedValue))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid short default");
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