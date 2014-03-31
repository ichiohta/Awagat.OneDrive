using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class SharedWith
    {
        [DataMember(Name = "access")]
        public string Access { get; set; }
    }
}
