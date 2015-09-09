using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Atom.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    [Flags]
    public enum QueryFlags
    {
        [EnumMember(Value = "none")]
        None = 0,

        [EnumMember(Value = "insert")]
        Insert = 1,

        [EnumMember(Value = "update")]
        Update = 2,

        [EnumMember(Value = "getOne")]
        GetOne = 4,
        
        [EnumMember(Value = "getBy")]
        GetBy = 8,

        [EnumMember(Value = "getAll")]
        GetAll = 16,

        [EnumMember(Value = "get")]
        Get = GetOne | GetBy | GetAll,

        [EnumMember(Value = "getMany")]
        BatchGet = 32,

        [EnumMember(Value = "all")]
        Defaults = Insert | Update | Get | BatchGet,

        [EnumMember(Value = "upsert")]
        Upsert = 64,

        [EnumMember(Value = "delete")]
        Delete = 128
    }
}
