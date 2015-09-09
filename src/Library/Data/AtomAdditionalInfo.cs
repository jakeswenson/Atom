using System;
using System.Collections.Generic;
using System.Linq;

namespace Atom.Data
{
    public class AtomAdditionalInfo : IMergable<AtomAdditionalInfo>
    {
        public AtomAdditionalInfo()
        {
            Temporal = new TemporalInfo();
            QueryTypes = new List<QueryFlags>();
        }

        public bool? HideId { set { HasHiddenPrimaryKey = value; } }

        public bool? HasHiddenPrimaryKey { get; set; }

        public bool? AutoGenId { get; set; }

        public bool? UseSoftDeletes { get; set; }

        public bool? UseWithNoLockHint { get; set; }

        internal bool ShouldGenerateCode()
        {
            return GenerateCode.GetValueOrDefault(defaultValue: true);
        }

        internal bool ShouldHidePrimaryKey()
        {
            return HasHiddenPrimaryKey ?? false;
        }
        
        public bool? SelectAfterInsert { get; set; }

        public TemporalInfo Temporal { get; set; }

        public List<QueryFlags> QueryTypes { get; set; }

        public string Schema { get; set; }

        public bool? FilterDeleted { get; set; }

        public bool? GenerateCode { get; set; }

        public bool? GenerateLookupData { get; set; }

        public void Merge(AtomAdditionalInfo defaults)
        {
            HasHiddenPrimaryKey = HasHiddenPrimaryKey ?? defaults.HasHiddenPrimaryKey;
            AutoGenId = AutoGenId ?? defaults.AutoGenId;
            FilterDeleted = FilterDeleted ?? defaults.FilterDeleted;
            UseSoftDeletes = UseSoftDeletes ?? defaults.UseSoftDeletes;
            UseWithNoLockHint = UseWithNoLockHint ?? defaults.UseWithNoLockHint;
            GenerateCode = GenerateCode ?? defaults.GenerateCode ?? true;
            Schema = Schema ?? defaults.Schema ?? Constants.DefaultSchema;
            GenerateLookupData = GenerateLookupData ?? defaults.GenerateLookupData;
            Temporal.Merge(defaults.Temporal);
            MergeQueryTypes(defaults);
        }

        private void MergeQueryTypes(AtomAdditionalInfo defaults)
        {
            if ((QueryTypes == null ||
                QueryTypes.Count == 0) &&
                defaults.QueryTypes != null)
            {
                QueryTypes = defaults.QueryTypes.ToList();
            }
        }

        public static AtomAdditionalInfo Default()
        {
            return new AtomAdditionalInfo();
        }
    }
}
