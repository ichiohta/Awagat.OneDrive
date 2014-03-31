using Awagat.OneDrive.Json;
using Awagat.OneDrive.OAuth2.Views;
using Awagat.OneDrive.Util;
using System;
using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Threading;

namespace Awagat.OneDrive.OAuth2
{
    public static partial class SessionUtility
    {
        #region Constants

        public const string RedirectUri  = "https://login.live.com/oauth20_desktop.srf";
        public const string TokenUri     = "https://login.live.com/oauth20_token.srf";

        private const string ScriptTemplate =
            "[System.Reflection.Assembly]::LoadFile('{0}'); " +
            "$window = New-Object -Type {1}; " +
            "$code = $window.Authenticate()";

        #endregion

        #region Helper methods

        private static string GetAuthorizationCode()
        {
            string script = string.Format(
                ScriptTemplate,
                Assembly.GetExecutingAssembly().Location,
                typeof(AuthWindow).FullName);

            using (var rs = RunspaceFactory.CreateRunspace())
            {
                rs.ApartmentState = ApartmentState.STA;
                rs.ThreadOptions = PSThreadOptions.ReuseThread;
                rs.Open();

                Pipeline pipe = rs.CreatePipeline();
                pipe.Commands.AddScript(script);
                pipe.Invoke();

                return rs.SessionStateProxy.GetVariable("code") as string;
            }
        }

        private static Session GetAccessToken(this string autorizationCode)
        {
            var profile = ClientProfileProvider.Default.Profile;

            var data = new Dictionary<string, string>()
            {
                { "code",          autorizationCode     },
                { "client_id",     profile.Id           },
                { "client_secret", profile.Secret       },
                { "redirect_uri",  RedirectUri          },
                { "grant_type",    "authorization_code" }
            };

            Token token = data.SendAndReceive<Token>(TokenUri);

            return new Session()
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                Expiration = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn)
            };
        }

        private static Session RefreshToken(this Session session)
        {
            var profile = ClientProfileProvider.Default.Profile;

            var data = new Dictionary<string, string>()
            {
                { "refresh_token", session.RefreshToken },
                { "client_id",     profile.Id           },
                { "client_secret", profile.Secret       },
                { "grant_type",    "refresh_token"      }
            };

            Token token = data.SendAndReceive<Token>(TokenUri);

            session.AccessToken  = token.AccessToken;
            session.RefreshToken = token.RefreshToken;
            session.Expiration   = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);

            return session;
        }

        #endregion

        #region Public methods

        public static Session StartSession()
        {
            return GetAuthorizationCode().GetAccessToken();
        }

        public static Session RefreshSession(this Session session)
        {
            return session.RefreshToken();
        }

        #endregion
    }
}
