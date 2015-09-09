using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberFloat : MemberType
    {
        public override string ValidateDefault(string value)
        {
            float f;
            if (float.TryParse(value, out f))
            {
                return value;
            }

            throw new Exception($"{value} is an invalid float default");
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
