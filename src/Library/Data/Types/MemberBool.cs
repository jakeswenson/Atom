using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberBool : MemberType
    {
        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                return false;
            }
        }

        public override string ValidateDefault(string value)
        {
            bool x;

            if (bool.TryParse(value, out x))
            {
                return x ? "(1)" : "(0)";
            }

            throw new Exception($"{value} is an invalid boolean default");
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}
