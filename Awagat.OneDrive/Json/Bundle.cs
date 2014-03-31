using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class Bundle
    {
        [DataMember(Name = "data")]
        public List<Item> Data { get; set; }
    }
}
