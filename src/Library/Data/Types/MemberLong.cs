using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberLong : MemberType
    {
        public override string ValidateDefault(string value)
        {
            long x;
            if (!long.TryParse(value, out x))
            {
                throw new Exception($"{value} is an invalid long default");
            }

            return value;
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
