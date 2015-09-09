using Atom.Data;

namespace Atom.Generation.Data
{
    public class SimpleReference : Reference
    {
        private readonly AtomModel _atomModel;

        public SimpleReference(AtomModel atomModel)
        {
            _atomModel = atomModel;
        }

        public override AtomModel ReferenceTarget
        {
            get
            {
                return _atomModel;
            }
        }

        public override string ToString()
        {
            return _atomModel.Name;
        }
    }
}
