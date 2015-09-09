using Atom.Data;

namespace Atom.Generation.Data
{
    public abstract class Reference
    {
        public string Name
        {
            get
            {
                return ReferenceTarget.Name;
            }
        }

        public abstract AtomModel ReferenceTarget { get; }

        public abstract override string ToString();
    }
}
