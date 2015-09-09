using Atom.Data.Serialization;
using Newtonsoft.Json;

namespace Atom.Data
{
    [JsonConverter(typeof(TemporalInfoConverter))]
    public class TemporalInfo : IMergable<TemporalInfo>
    {
        public bool? HasTemporal { get; set; }

        public AtomMemberSortDirection? CreatedOnSort { get; set; }

        public AtomMemberSortDirection? LastModifiedSort { get; set; }

        public bool? IndexCreatedOn { get; set; }
        public bool? IndexLastModified { get; set; }

        public bool? LastModified { get; set; }
        public bool? CreatedOn { get; set; }

        public bool? UseDateTime2 { get; set; }
        public int? DateTime2Precision { get; set; }

        public void Merge(TemporalInfo defaults)
        {
            HasTemporal = HasTemporal ?? defaults.HasTemporal;
            CreatedOnSort = CreatedOnSort ?? defaults.CreatedOnSort;
            LastModifiedSort = LastModifiedSort ?? defaults.LastModifiedSort;
            IndexLastModified = IndexLastModified ?? defaults.IndexLastModified;
            IndexCreatedOn = IndexCreatedOn ?? defaults.IndexCreatedOn;
            LastModified = LastModified ?? defaults.LastModified;
            CreatedOn = CreatedOn ?? defaults.CreatedOn;
            UseDateTime2 = UseDateTime2 ?? defaults.UseDateTime2;
            DateTime2Precision = DateTime2Precision ?? defaults.DateTime2Precision;
        }
    }
}
