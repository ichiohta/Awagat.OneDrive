using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class ClientProfile
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "secret")]
        public string Secret { get; set; }
    }
}
