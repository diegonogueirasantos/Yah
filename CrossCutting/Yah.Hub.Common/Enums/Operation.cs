using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace Yah.Hub.Common.Enums
{
    public enum Operation
    {
        [EnumMember(Value = "Insert")]
        Insert,
        [EnumMember(Value = "Update")]
        Update,
        [EnumMember(Value = "Delete")]
        Delete
    }
}
