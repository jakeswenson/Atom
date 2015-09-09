namespace Atom.Data.Projections
{
    public class AliasedAtomMemberInfo
    {
        public AtomMemberInfo Member { get; set; }
        
        public string Name { get; set; }

        public bool HasAlias
        {
            get { return Name != Member.Name; }
        }

        public static AliasedAtomMemberInfo FromAtomMemberInfo(AtomMemberInfo src)
        {
            return new AliasedAtomMemberInfo
                   {
                       Name = src.Name,
                       Member = src
                   };
        }

        public override string ToString()
        {
            return string.Format("Member: {0}, Name: {1}", Member, Name);
        }
    }
}
