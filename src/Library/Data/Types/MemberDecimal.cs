using System;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberDecimal : MemberType
    {
        public override string ValidateDefault(string value)
        {
            decimal parsedValue;
            if (decimal.TryParse(value, out parsedValue))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid decimal default");
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