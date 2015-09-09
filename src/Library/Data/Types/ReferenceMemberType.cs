using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    class ReferenceMemberType : MemberType
    {
        public MemberType ReferenceType { get; set; }

        private void EnsureReferenceType()
        {
            if (ReferenceType == null)
            {
                throw new InvalidOperationException("No reference type has been set on this type. Make sure RelateLinks was called.");
            }
        }

        public override string ValidateDefault(string value)
        {
            EnsureReferenceType();
            return ReferenceType.ValidateDefault(value);
        }

        public override TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor)
        {
            EnsureReferenceType();
            return ReferenceType.Accept(defaultTypeVisitor);
        }

        public override bool CanSupplyDefaultCreationValue
        {
            get
            {
                EnsureReferenceType();
                return ReferenceType.CanSupplyDefaultCreationValue;
            }
        }

        public override string GetAdditionalSqlAnnotation()
        {
            EnsureReferenceType();
            return ReferenceType.GetAdditionalSqlAnnotation();
        }
    }
}
