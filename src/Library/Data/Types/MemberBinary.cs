using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberBinary : MemberType
    {
        private readonly int _length;

        private readonly bool _variableSize;

        public MemberBinary(int length, bool variableSize)
        {
            _length = length;
            _variableSize = variableSize;
        }

        public int Length
        {
            get
            {
                return _length;
            }
        }

        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                return false;
            }
        }

        public bool VariableSize
        {
            get
            {
                return _variableSize;
            }
        }

        public override string ValidateDefault(string value)
        {
            return value;
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}
