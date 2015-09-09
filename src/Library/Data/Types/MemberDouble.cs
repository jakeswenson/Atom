using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberDouble : MemberType
    {
        public override string ValidateDefault(string value)
        {
            double parsedValue;
            if (double.TryParse(value, out parsedValue))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid double default");
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