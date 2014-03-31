using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Controls;

namespace Awagat.OneDrive.OAuth2.Views
{
    public partial class AuthWindow : Window
    {
        #region Constants

        private const string FormatUri        = "{0}?scope={1}&redirect_uri={2}&response_type=code&client_id={3}";
        private const string AuthorizationUri = "https://login.live.com/oauth20_authorize.srf";
        private const string Scope            = "wl.offline_access wl.skydrive_update";
        private const string ApplicationName  = "Microsoft Account Autentication";

        private static readonly Regex RxAuthorizationCode = new Regex(@"code=([^&]+)");

        #endregion

        #region Properties

        public string AuthorizationCode { get; set; }

        #endregion

        public AuthWindow()
        {

            InitializeComponent();
        }

        public string Authenticate()
        {
            string authUri = string.Format(
                FormatUri,
                AuthorizationUri,
                HttpUtility.UrlEncode(Scope),
                SessionUtility.RedirectUri,
                ClientProfileProvider.Default.Profile.Id);

            Root.Title = ApplicationName;

            WebBrowser.Navigate(authUri);
            ShowDialog();

            return AuthorizationCode;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            WebBrowser.Width  = e.NewSize.Width;
            WebBrowser.Height = e.NewSize.Height;
        }

        protected void WebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            Match match = RxAuthorizationCode.Match(e.Uri.AbsoluteUri);

            if (e.Uri.AbsoluteUri.StartsWith(SessionUtility.RedirectUri) && match.Success)
            {
                AuthorizationCode = match.Groups[1].Value;
                this.Close();
            }
        }
    }
}
