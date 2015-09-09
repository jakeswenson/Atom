using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public class MemberText : MemberType
    {
        private readonly bool _unicode;

        public MemberText(int length, bool unicode = false)
        {
            _unicode = unicode;

            Length = length;
        }

        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                return false;
            }
        }

        public int Length { get; private set; }

        public bool IsUnicode
        {
            get { return _unicode; }
        }

        public override string ValidateDefault(string value)
        {
            if (value.Length > Length)
            {
                throw new Exception($"Default value '{value}' is an invalid text default because its length is larger than the column size ({Length})");
            }

            return value;
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            return defaultTypeVisitor.Visit(this);
        }
    }
}
