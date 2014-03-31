using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    public class CopyMoveOperation
    {
        [DataMember(Name = "destination")]
        public string Destination { get; set; }
    }
}
