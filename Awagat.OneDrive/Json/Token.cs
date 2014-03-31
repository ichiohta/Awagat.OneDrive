using System.Runtime.Serialization;

namespace Awagat.OneDrive.Json
{
    [DataContract]
    internal class Token
    {
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }

        [DataMember(Name = "expires_in")]
        public int ExpiresIn { get; set; }

        [DataMember(Name = "scope")]
        public string Scope { get; set; }

        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }

        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }

        [DataMember(Name = "authentication_token")]
        public string AuthenticationToken { get; set; }
    }
}
