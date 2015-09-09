using System;

namespace Atom.Exceptions
{
    public class UnknownProjectionAtomException : Exception
    {
        public UnknownProjectionAtomException(string str) : base(str)
        {            
        }
    }
}
