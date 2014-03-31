using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class UpdateOperation
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}
