using System;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using BGT.Database.Extensions;
using BGT.GlobalData;
using System.Data;
using Carnival.Infrastructure.Logging;
using Carnival.Infrastructure.Security.Encryption;
using Carnival.Infrastructure.Utils;
using GlobalData;
using Portal.Security;
using SingleSignon.Services;
using System.Collections.Generic;

namespace Sitecore.Website.Controls.Login
{

    /// <summary>
    /// Summary description for LoginSublayout
    /// </summary>
    public partial class Login : System.Web.UI.UserControl
    {
        private static readonly Guid _myGuid = Guid.Parse("E6EF9D5D-9F38-4BC5-A61E-8DB1AA854051");
        private const string COOKIE_USERNAME = "GoCCLUsername";
        private const string BOOKING_ENGINE_IMPERSONATION_COOKIE_NAME = "Uid";
        private const string VALID_HOSTS_KEY = "ValidHosts";

        private string _buttonText;
        public string ButttonText { get { return string.IsNullOrEmpty(_buttonText) ? (_buttonText = BGT.CodeLibrary.GenericFunctions.GetButtonText("{0349B4B8-868B-48C4-91B4-12729D51E9C9}")) : _buttonText; } }
        public string RegisterGreeting { get; set; }
        public string RegisterButttonText { get; set; }
        public string RegistrationUrl { get; set; }

        //Variable to Check Agency Restriction
        public string AgencyRestrictionForDoamin = System.Configuration.ConfigurationManager.AppSettings["AgencyRestrictionForDoamin"] ?? string.Empty;
        public string AgencyRestrictionAllowPhoneAreaCodes = System.Configuration.ConfigurationManager.AppSettings["AgencyRestrictionAllowPhoneAreaCodes"] ?? string.Empty;
        public string AgencyRestrictionNotAllowPhoneAreaCodes = System.Configuration.ConfigurationManager.AppSettings["AgencyRestrictionNotAllowPhoneAreaCodes"] ?? string.Empty;
        public IEnumerable<string> ValidHosts { get; set; }

        private readonly SessionService _sessionService = new SessionService();

        private void Page_Load(object sender, EventArgs e)
        {

            if (!Page.IsPostBack)
            {
                var cookieUserName = Request.Cookies[COOKIE_USERNAME];
                if (cookieUserName != null)
                {
                    txtLogin.Value = Server.HtmlDecode(cookieUserName.Value);
                    remember.Checked = true;
                }

                Sitecore.Data.Items.Item tempItem = Sitecore.Context.Item;
                tempItem = Sitecore.Context.Database.Items["/sitecore/content/Home/Login"];
                tempItem = BGT.SitecoreLibrary.ContentUtil.GetLanguageVersion(tempItem);

                var login = new BGT.SitecoreLibrary.SectionTemplates.Login.LoginItem(tempItem);
                RegisterButttonText = BGT.CodeLibrary.GenericFunctions.GetButtonText("{CF0330A7-55B6-42A2-BEE7-47F343D38A5B}");

                RegisterGreeting = login.RegistrationText.Rendered;
                RegistrationUrl = login.RegistrationPage.Url;

                if (Request.IsAuthenticated || IsBookingEngineImpersonating())
                {
                    pnlLogin.Visible = false;
                    RedirectToReturnUrl();
                }
                else
                    pnlLogin.Visible = true;
            }
        }

        private bool IsBookingEngineImpersonating()
        {
            var impersonationCookie = HttpContext.Current.Request.Cookies[BOOKING_ENGINE_IMPERSONATION_COOKIE_NAME];

            return impersonationCookie != null && !string.IsNullOrWhiteSpace(impersonationCookie.Value);
        }

        public void ButtonLogin_Click(Object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                LoginUser();
            }
        }


        public void LoginUser()
        {
            BGT.Database.UserManagement objLogin = new BGT.Database.UserManagement();

            CleanSession();

            AgentAcctInfoDto dtUserInfo = null;
            var userLogin = Server.HtmlEncode(txtLogin.Value);
            var userPasswordEncrypted = Server.HtmlEncode(txtPass.Value.ToLower()).ToSecureString().SymmetricalEncrypt().ToSecureString();
            string userValidation = objLogin.LoginUser(userLogin, userPasswordEncrypted, out dtUserInfo);

            if ((userValidation ?? string.Empty).ToLower() == "valid")
            {
                #region Agency Restriction check
                GlobalSession.AgencyId = dtUserInfo.AgencyID;

                var agencyRestrictionForDomain = (System.Configuration.ConfigurationManager.
                    AppSettings["AgencyRestrictionForDoamin"] ?? string.Empty).ToUpper();

                if (!string.IsNullOrEmpty(agencyRestrictionForDomain) &&
                    HttpContext.Current.Request.Url.Host.ToUpper().Contains(agencyRestrictionForDomain))
                {
                    try
                    {
                        string AgencyAreaCode = dtUserInfo.AgencyPhoneAreaCode;
                        bool IsvalidAgency = false;

                        if (AgencyRestrictionNotAllowPhoneAreaCodes.Equals("*"))
                        {
                            // Means allow only AgencyRestrictionAllowPhoneAreaCodes - UK Scenario
                            if (AgencyRestrictionAllowPhoneAreaCodes.Contains(AgencyAreaCode))
                                IsvalidAgency = true;
                        }
                        else if (AgencyRestrictionAllowPhoneAreaCodes.Equals("*"))
                        {
                            // Means Allow every one except AgencyRestrictionNotAllowPhoneAreaCodes - US Scenario 
                            if (!AgencyRestrictionNotAllowPhoneAreaCodes.Contains(AgencyAreaCode))
                                IsvalidAgency = true;
                        }
                        if (!IsvalidAgency)
                            userValidation = ErrorConstants.INVALID_AGENCY_LOCALITY;
                    }
                    catch (Exception ex)
                    {
                        string message = ErrorConstants.ERROR_VALIDATING_AGENCY_LOCALITY;
                        Sitecore.Diagnostics.Log.Error(message, ex, this);
                        CCLLogger.Log(message, ex, _myGuid, TraceEventType.Error);
                        userValidation = message;
                    }
                }
                #endregion
            }
            if ((userValidation ?? string.Empty).ToLower() == "valid")
            {
                // Continue with the rest of the validations
                GlobalSession.AgencyId = dtUserInfo.AgencyID;
                string userRole = dtUserInfo.Rights.ToString(CultureInfo.InvariantCulture);
                string loginId = dtUserInfo.LoginID;
                string fullName = string.Join(" ", new[]
                                                       {
                                                           dtUserInfo.FirstName,
                                                           dtUserInfo.LastName
                                                       });
                string AreaCode = dtUserInfo.AreaCode;

                Auth.SetAuthentication(userRole, loginId, fullName);

                // Store Authenticated user info in session
                GlobalSession.UserFullName = string.Concat(dtUserInfo.FirstName.ToLower(), " ", dtUserInfo.LastName.ToLower());
                GlobalSession.RightsAccessLevel = dtUserInfo.Rights.ToString(CultureInfo.InvariantCulture);
                GlobalSession.AgentProfile = objLogin.GetAgentByLoginId(loginId);
                GlobalSession.LoginId = userLogin;

                if (SingleSignonIsEnabled())
                    InitializeSsoSession(userLogin);

                if (!RedirectToReturnUrl())
                {
                    Response.Redirect("/", true);
                }
            }
            else
            {
                LblMessage.Text = userValidation;
                CleanSession();
            }
        }

        private static bool SingleSignonIsEnabled()
        {
            bool isenabled = false;

            bool.TryParse(WebConfigurationManager.AppSettings["IsSingleSignonEnabled"], out isenabled);

            return isenabled;
        }

        private void InitializeSsoSession(string username)
        {
            try
            {
                Session[SessionService.SINGLE_SIGNON_SESSION_TOKEN] = _sessionService.InsertEntryIntoSession(username);
            }
            catch (Exception ex)
            {
                CCLLogger.Log("Unable to initiate the SSO user session.", ex, Guid.NewGuid(), TraceEventType.Error);
            }
        }

        private void CleanSession()
        {
            Session.RemoveAll();
        }

        private HttpCookie GetResponseCookie(string cookieName)
        {
            var result = Response.Cookies[cookieName];
            if (result == null)
            {
                result = new HttpCookie(cookieName);
                Response.Cookies.Add(result);
            }
            return result;
        }

        private void SetRememberMeCookie()
        {
            var cookieUsername = GetResponseCookie(COOKIE_USERNAME);

            if (remember.Checked)
            {
                cookieUsername.Value = Server.HtmlEncode(txtLogin.Value);
            }
            else
            {
                var expiration = DateTime.Now.AddDays(-10);
                cookieUsername.Value = string.Empty;
                cookieUsername.Expires = expiration;
            }
        }

        private bool RedirectToReturnUrl()
        {
            Uri tempUri = new Uri("http://" + Request.Url.Host + Request.RawUrl.ToString());

            string sQ = tempUri.Query;

            if (!string.IsNullOrEmpty(sQ))
                if (sQ.ToLower().Contains("returnurl="))
                    sQ = sQ.Substring(sQ.ToLower().IndexOf("returnurl=") + 10);

            if (!string.IsNullOrEmpty(sQ))
            {
                SetRememberMeCookie();

                sQ = System.Web.HttpUtility.UrlDecode(sQ);
                var validHosts = GetValidHosts();
                if (IsValidUrl(sQ, validHosts))
                {
                    Response.Redirect(sQ, true);
                }

            }
            return false;
        }

        private bool IsValidUrl(string url, IEnumerable<string> validHosts)
        {
            bool isValid = true;
            Uri absoluteUri;
            if (string.IsNullOrEmpty(url))
            {
                isValid = false;
            }
            else if (Uri.TryCreate(url, UriKind.Absolute, out absoluteUri))
            {
                isValid = string.Equals(this.Request.Url.Host, absoluteUri.Host, StringComparison.OrdinalIgnoreCase)
                    || (validHosts != null && validHosts.Any(x => string.Equals(x, absoluteUri.Host,
                            StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                isValid = Uri.IsWellFormedUriString(url, UriKind.Relative);
                isValid = !url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
                    && !url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
                    && Uri.IsWellFormedUriString(url, UriKind.Relative);
            }

            return isValid;
        }

        private IEnumerable<string> GetValidHosts()
        {
            var value = ConfigurationManager.AppSettings[VALID_HOSTS_KEY] ?? string.Empty;
            return value.Split(';');
        }
    }
}