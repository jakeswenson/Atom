namespace Atom.Data
{
    public static class AtomMembers
    {
        public static AtomMemberInfo DateTime(string name)
        {
            return new AtomMemberInfo
            {
                Name = name,
                Type = "datetime",
            };
        }
        public static AtomMemberInfo DateTime2(string name, int? precision = null)
        {
            return new AtomMemberInfo
            {
                Name = name,
                Precision = precision,
                Type = "datetime2",
            };
        }

        public static AtomMemberInfo Bool(string name)
        {
            return new AtomMemberInfo
            {
                Name = name,               
                Type = "bool"
            };
        }

        public static AtomMemberInfo WithDefault(this AtomMemberInfo memberInfo, SqlDefaultValue defaultValue)
        {
            memberInfo.DefaultValue = defaultValue.SqlRepresentation();

            return memberInfo;
        }

        public static AtomMemberInfo NotUpdateable(this AtomMemberInfo memberInfo)
        {
            return memberInfo.AddFlags(MemberFlags.NotUpdateable);
        }

        public static AtomMemberInfo Long(string name)
        {
            return new AtomMemberInfo
            {
                Name = name,
                Type = "long",
            };
        }        

        public static AtomMemberInfo Key(this AtomMemberInfo memberInfo)
        {
            memberInfo.Unique = true;
            memberInfo.Queryable = true;
            memberInfo.IsPrimary = true;
            memberInfo.AddFlags(MemberFlags.PrimaryKey);

            return memberInfo;
        }

        internal static AtomMemberInfo Generated(this AtomMemberInfo memberInfo, MemberFlags additionalFlags = MemberFlags.None)
        {
            return memberInfo.AddFlags(MemberFlags.Generated | additionalFlags);
        }

        internal static AtomMemberInfo Flag(this AtomMemberInfo memberInfo, MemberFlags additionalFlags = MemberFlags.None)
        {
            return memberInfo.AddFlags(additionalFlags);
        }
    }
}
