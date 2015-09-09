using System.Collections.Generic;

namespace Atom.Data
{
    public class AtomDefaults
    {
        public AtomDefaults()
        {
            Roles = new List<DatabaseRole>();
            AdditionalInfo = new AtomAdditionalInfo();
        }

        public AtomAdditionalInfo AdditionalInfo { get; set; }

        public List<DatabaseRole> Roles { get; set; }
    }
}
