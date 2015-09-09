using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public class OrderedAtomMemberConverter : JsonCreationConverter<OrderedAtomMembers>
    {
        public override bool CanWrite
        {
            get { return true; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var atomMembers = value as OrderedAtomMembers;

            writer.WriteStartObject();

            foreach (var member in atomMembers)
            {
                writer.WritePropertyName(member.Name);

                serializer.Serialize(writer, member);
            }

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader) as JObject;

            var target = new OrderedAtomMembers();

            foreach (var item in token.Properties())
            {
                string name = item.Name;

                var member = new AtomMemberInfoConverter().ReadJson(item.Value.CreateReader(), null, null, serializer) as AtomMemberInfo;

                member.Name = name;

                target.Add(member);
            }

            return target;
        }

        protected override OrderedAtomMembers After(OrderedAtomMembers data)
        {
            return data;
        }
    }
}
