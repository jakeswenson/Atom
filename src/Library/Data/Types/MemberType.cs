using System;
using Atom.Generation.Generators.Code;
using Atom.Types;

namespace Atom.Data.Types
{
    public abstract class MemberType
    {
        public abstract string ValidateDefault(string value);

        public abstract TResult Accept<TResult>(ITypeVisitor<TResult> defaultTypeVisitor);

        public virtual string GetAdditionalSqlAnnotation()
        {
            return string.Empty;
        }

        public sealed override string ToString()
        {
            return GetType().Name;
        }

        public abstract bool CanSupplyDefaultCreationValue { get;  }
    }
}
