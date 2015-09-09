using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Atom.Data.Types;
using Atom.Data.Serialization;

namespace Atom.Data
{
    [JsonConverter(typeof(AtomMemberInfoConverter))]
    public class AtomMemberInfo
    {
        private MemberFlags _flags;

        public string Type { get; set; }

        public bool Queryable { get; set; }

        public bool Unique { get; set; }

        public bool IsPrimary { get; set; }

        public AtomMemberSortDirection? SortDirection { get; set; }

        internal bool IsAltKey { get; set; }

        public bool Optional { get; set; }

        public AtomReference Reference { get; set; }

        public int? Length { get; set; }

        public bool? Unicode { get; set; }

        public int? Precision { get; set; }

        public string DefaultValue { get; set; }

        public List<string> Groups { get; set; }

        public bool Key
        {
            get { return IsAltKey; }
            [UsedImplicitly] set
            {
                IsAltKey = value;
                if (value)
                {
                    AddFlags(MemberFlags.Key);
                }
                else
                {
                    RemoveFlag(MemberFlags.Key);
                }
            }
        }

        internal bool HasReference
        {
            get { return Reference != null; }
        }

        internal MemberType MemberType { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        internal AtomModel Atom { get; set; }

        public bool VariableSize { get; set; }

        public override string ToString()
        {
            return string.Format("Name: {0}, MemberType: {1}", Name, MemberType);
        }

        internal bool HasFlag(MemberFlags flags)
        {
            return _flags.HasFlag(flags);
        }

        internal AtomMemberInfo AddFlags(MemberFlags flags)
        {
            _flags |= flags;
            return this;
        }

        private void RemoveFlag(MemberFlags flagsToRemove)
        {
            _flags &= ~flagsToRemove;
        }
    }
}
