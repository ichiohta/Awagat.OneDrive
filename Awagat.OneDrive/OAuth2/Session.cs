
using System;
using System.Management.Automation;

namespace Awagat.OneDrive.OAuth2
{
    public class Session
    {
        #region Properties

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public DateTimeOffset Expiration { get; set; }

        public bool IsExpired
        {
            get
            {
                return DateTimeOffset.UtcNow > Expiration;
            }
        }

        #endregion

        #region .ctor

        public Session() { }

        public Session(string accessToken, string refreshToken)
        {
            AccessToken  = accessToken;
            RefreshToken = refreshToken;
        }

        #endregion
    }
}
