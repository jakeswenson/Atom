using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Atom.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AtomMemberSortDirection
    {
        Ascending,
        Descending,
        Asc = Ascending,
        Desc = Descending,
    }
}
