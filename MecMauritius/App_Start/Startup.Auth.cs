using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using Microsoft.Owin.Security.OpenIdConnect;
using MecMauritius.Models;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Owin.Security.MicrosoftAccount;
using Microsoft.Owin.Security.Facebook;
using Owin.Security.Providers.Yahoo;

namespace MecMauritius
{
    public partial class Startup
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864

        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];

        string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);



            app.UseOpenIdConnectAuthentication(
                new OpenIdConnectAuthenticationOptions
                {
                    ClientId = clientId,
                    Authority = authority,
                    PostLogoutRedirectUri = postLogoutRedirectUri,
                    RedirectUri = postLogoutRedirectUri,
                    Notifications = new OpenIdConnectAuthenticationNotifications
                    {
                        AuthenticationFailed = context =>
                        {
                            context.HandleResponse();
                            context.Response.Redirect("/Error?message=" + context.Exception.Message);
                            return Task.FromResult(0);
                        }
                    }
                });

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "1708898b-9804-4028-b8fc-6328888aa899",
            //    clientSecret: "eR7cGoWspSjsZ1Gueipkxcm");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");



            app.UseFacebookAuthentication(
               appId: "1728713387393341",
               appSecret: "0f0eec3af5282d30eac6b40b3530e3b3");

            app.UseYahooAuthentication(
                "dj0yJmk9NFN5R3RhMEx1RTZmJmQ9WVdrOU1sQTVaWGRCTlRZbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmeD02ZA--",
                "555caa087778e9ab3f3de60bb07673929c48413b");

            //var facebookOptions = new FacebookAuthenticationOptions()
            //{
            //    AppId = "1821725144810201",
            //    AppSecret = "9e44baa0a16bd29c34ad5bb0d2632d75",
            //    BackchannelHttpHandler = new FacebookBackChannelHandler(),
            //    UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,email"
            //};

            //facebookOptions.Scope.Add("email");
            //app.UseFacebookAuthentication(facebookOptions);

            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountAuthenticationOptions()
            {
                ClientId = "3054e163-8a73-453d-ba45-4516eecedfca",
                ClientSecret = "RehnECU8DHyahFamGL9B4Vd",
                Scope = { "wl.basic", "wl.emails" }
            });

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "635955535955-7inojsgo8ugtg5jkodg0ek7krlgvnulq.apps.googleusercontent.com",
                ClientSecret = "qc8qg8tGYKd8ZFbVccMW6Q_5"
            });
        }
    }
}