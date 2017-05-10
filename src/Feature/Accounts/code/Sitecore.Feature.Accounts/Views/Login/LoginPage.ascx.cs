using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using BGT.GlobalData;

namespace Sitecore.Website.Controls.Login
{

    /// <summary>
    /// Summary description for LoginpageSublayout
    /// </summary>
    public partial class Loginpage : System.Web.UI.UserControl
    {

        public string ImagePath { get; set; }
        public string AltText { get; set; }

        private void Page_Load(object sender, EventArgs e)
        {
            //ifuser is already logged in Please redirect to home page or returnurl page
            if (!this.Page.IsPostBack)
                if (Request.IsAuthenticated)
                {
                    Uri tempUri = new Uri("http://" + Request.Url.Host + Request.RawUrl.ToString());
                    string sQ = tempUri.Query;

                    if (!string.IsNullOrEmpty(sQ))
                        if (sQ.ToLower().Contains("returnurl="))
                            sQ = sQ.Substring(sQ.ToLower().IndexOf("returnurl=") + 10);

                    if (!string.IsNullOrEmpty(sQ))
                    {
                        sQ = System.Web.HttpUtility.UrlDecode(sQ);
                        if (!string.IsNullOrEmpty(sQ))
                        {
                            // if (sQ.ToLower().Contains(System.Web.HttpContext.Current.Request.Url.Host.ToLower()) || sQ.ToLower().StartsWith("/"))
                            Response.Redirect(sQ, true);
                        }

                    }
                    Response.Redirect("/");
                }

            // Put user code to initialize the page here

            Sitecore.Data.Items.Item tempItem = Sitecore.Context.Item;
            tempItem = BGT.SitecoreLibrary.ContentUtil.GetLanguageVersion(tempItem);

            if (Sitecore.Context.Item.TemplateID.ToString() == "{260A7BB0-BDAA-4E81-A2DE-D687CDA5F33A}")
            {

                BGT.SitecoreLibrary.SectionTemplates.Login.LoginItem LoginPage = new BGT.SitecoreLibrary.SectionTemplates.Login.LoginItem(tempItem);
                ImagePath = LoginPage.BackgroundImage.MediaUrl;
                AltText = !string.IsNullOrEmpty(LoginPage.BackgroundImage.MediaItem.Alt) ? LoginPage.BackgroundImage.MediaItem.Alt : "";

            }
            else if (Sitecore.Context.Item.TemplateID.ToString() == "{ABD7C33E-F098-49D7-A111-093086BBC945}")
            {

                BGT.SitecoreLibrary.SectionTemplates.Login.ForgotPasswordItem ForgotPasswordPage = new BGT.SitecoreLibrary.SectionTemplates.Login.ForgotPasswordItem(tempItem);
                ImagePath = ForgotPasswordPage.BackgroundImage.MediaUrl;
                AltText = !string.IsNullOrEmpty(ForgotPasswordPage.BackgroundImage.MediaItem.Alt) ? ForgotPasswordPage.BackgroundImage.MediaItem.Alt : "";
            }
            else
            {

                tempItem = Sitecore.Context.Database.GetItem(new Sitecore.Data.ID("{F238BA3F-FF8A-430E-AAFD-09EAAC241238}"));
                tempItem = BGT.SitecoreLibrary.ContentUtil.GetLanguageVersion(tempItem);

                BGT.SitecoreLibrary.SectionTemplates.Login.LoginItem LoginPage = new BGT.SitecoreLibrary.SectionTemplates.Login.LoginItem(tempItem);
                ImagePath = LoginPage.BackgroundImage.MediaUrl;
                AltText = !string.IsNullOrEmpty(LoginPage.BackgroundImage.MediaItem.Alt) ? LoginPage.BackgroundImage.MediaItem.Alt : "";
            }


        }
    }
}