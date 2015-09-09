using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atom.Data.Serialization
{
    public abstract class JsonCreationConverter<T> : JsonConverter
        where T : new()
    {
        
        protected abstract T After(T data);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                        object existingValue,
                                        JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            var target = Deserialize(serializer, token);

            return After(target);
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        protected virtual T Deserialize(JsonSerializer serializer, JToken token)
        {
            T target = new T();
            
            serializer.Populate(token.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer,
                                       object value,
                                       JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
