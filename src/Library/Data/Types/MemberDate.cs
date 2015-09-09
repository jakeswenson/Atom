using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberDate : MemberType
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
            if (value != "getutcdate()")
            {
                throw new Exception($"{value} is an invalid sql datetime default. Try 'getutcdate()'");
            }

            return value;
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}
