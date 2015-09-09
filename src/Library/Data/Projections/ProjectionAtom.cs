using System.Collections.Generic;

namespace Atom.Data.Projections
{
    public class ProjectionAtom
    {
        public ProjectionAtom(Dictionary<string, List<string>> selects)
        {
            Tables = new Dictionary<string, ProjectionInfo>();

            foreach (var item in selects)
            {
                Tables[item.Key] = new ProjectionInfo
                {
                    SelectMembers = item.Value
                };
            }
        }

        public ProjectionAtom()
        {
        }

        public Dictionary<string, ProjectionInfo> Tables { get; set; }

        public string Name { get; set; }

        public bool NonDeleted { get; set; }

        public bool IgnoreConflicts { get; set; }
    }

    public enum FilterValue
    {
        True,
        False,
        Parameter
    }

    public class QueryProjectionAtom : ProjectionAtom
    {
        public Dictionary<FilterValue, List<string>> Filters { get; set; }
    }

    public class ViewProjectionAtom : ProjectionAtom
    {
    }
}
