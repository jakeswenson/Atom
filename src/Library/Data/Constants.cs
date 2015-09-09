namespace Atom.Data
{
    public static class Constants
    {
        public const string BigIntTableType = "AtomBigIntTableType";
        public const string GuidTableType = "AtomGuidTableType";

        public static class Members
        {
            public const string CreatedAtDateTime = "CreatedDateTimeUtc";
            public const string LastModifiedDateTime = "LastModifiedDateTimeUtc";
            public const string IsDeleted = "IsDeleted";
        }

        public static readonly string[] CreatedOnDateTimePotentialNames = { Members.CreatedAtDateTime };

        public static readonly string[] LastModifiedOnOnDateTimePotentialNames = { Members.LastModifiedDateTime };

        public const string DefaultSchema = "dbo";
    }
}
