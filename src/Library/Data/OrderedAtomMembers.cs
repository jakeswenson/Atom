using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Atom.Data.Serialization;
using System;

namespace Atom.Data
{
    [JsonConverter(typeof(OrderedAtomMemberConverter))]
    public class OrderedAtomMembers : KeyedCollection<string, AtomMemberInfo>
    {
        public OrderedAtomMembers()
        {
        }

        protected override string GetKeyForItem(AtomMemberInfo item)
        {
            return item.Name;
        }

        protected override void InsertItem(int index, AtomMemberInfo item)
        {
            if (Contains(GetKeyForItem(item)))
            {
                throw new ArgumentException($"A member with the key '{item.Name}' already exists.", paramName: nameof(item));
            }

            base.InsertItem(index, item);
        }
    }
}
