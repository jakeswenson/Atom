using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberDateTime : MemberType
    {
        public bool UseDateTime2 { get; private set; }

        public int DateTime2Precision { get; private set; }

        public MemberDateTime(bool useDateTime2 = false, int dateTime2Precision = 7)
        {
            UseDateTime2 = useDateTime2;

            if (dateTime2Precision < 0 || dateTime2Precision > 7)
            {
                throw new ArgumentException("Precision must be between 0 and 7");
            }

            DateTime2Precision = dateTime2Precision;
        }

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
