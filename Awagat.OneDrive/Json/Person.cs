using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class Person
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
