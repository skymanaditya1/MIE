using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MecMauritius.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.Owin.Security.OpenIdConnect;

namespace MecMauritius.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        ApplicationDbContext context;

        public AccountController()
        {
            context = new ApplicationDbContext();
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;

        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();

            // var account = new AccountController();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        //{
        //    // Require that the user has already logged in via username/password or external login
        //    if (!await SignInManager.HasBeenVerifiedAsync())
        //    {
        //        return View("Error");
        //    }
        //    return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/VerifyCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // The following code protects for brute force attacks against the two factor codes. 
        //    // If a user enters incorrect codes for a specified amount of time then the user account 
        //    // will be locked out for a specified amount of time. 
        //    // You can configure the account lockout settings in IdentityConfig
        //    var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
        //    switch (result)
        //    {
        //        case SignInStatus.Success:
        //            return RedirectToLocal(model.ReturnUrl);
        //        case SignInStatus.LockedOut:
        //            return View("Lockout");
        //        case SignInStatus.Failure:
        //        default:
        //            ModelState.AddModelError("", "Invalid code.");
        //            return View(model);
        //    }
        //}

        //
        // GET: /Account/Register
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            var rolesList = context.Roles.Where(u => !u.Name.Contains("Admin")).ToList();
            ViewBag.UserRoles = rolesList;
            return View();
        }

        // Method to generate school names for a selected zone
        [HttpGet]
        public JsonResult GetSchools(string id)
        {
            SchoolModel schoolModel = SchoolService.GenerateSchools(id);

            return Json(new SelectList(schoolModel.Schoollist, "Value", "Text"));
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, firstName = model.Firstname, lastName = model.Lastname, Birthdate = model.Birthdate };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    string role = educatorCheck(user.Id, model.UserRoles);
                    await this.UserManager.AddToRoleAsync(user.Id, role);

                    updateSchoolandZone(user.Id, model.Zone, model.School);

                    return RedirectToAction("Index", "Home");
                }

                //ViewBag.Name = new SelectList(context.Roles.Where(u => !u.Name.Contains("Admin"))
                //                          .ToList(), "Name", "Name");

                //List<SelectListItem> roles = new List<SelectListItem>();
                //roles.Add(new SelectListItem
                //{
                //    Text = "Select Role",
                //    Value = "Select Role",
                //    Selected = true
                //});
                //roles.Add(new SelectListItem { Text = "Student", Value = "Student" });
                //roles.Add(new SelectListItem { Text = "Educator", Value = "Educator" });

                //ViewBag.Roles = roles;

                ViewBag.UserRoles = context.Roles.Where(u => !u.Name.Contains("Admin")).ToList();

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // Method to update userid, categoryid, userrole, zone and school
        private static void updateSchoolZoneCategoryRole(string userid, int zoneid, int schoolid, string userroleid, string categoryid)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("addSchoolZoneforUser", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userId", userid);
                    sqlCommand.Parameters.AddWithValue("@zoneId", zoneid);
                    sqlCommand.Parameters.AddWithValue("@schoolId", schoolid);
                    sqlCommand.Parameters.AddWithValue("@categoryId", categoryid);
                    sqlCommand.Parameters.AddWithValue("@userroleId", userroleid);
                    sqlConnection.Open();
                    if (sqlCommand.ExecuteNonQuery() > 0)
                    {
                        Console.WriteLine("Sql query ran successfully");
                    }
                    else
                    {
                        Console.WriteLine("Sql query ran into error");
                    }
                }
            }
        }

        private static void updateSchoolandZone(string userId, int zoneId, int schoolId)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("addSchoolZoneforUser", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userId", userId);
                    sqlCommand.Parameters.AddWithValue("@zoneId", zoneId);
                    sqlCommand.Parameters.AddWithValue("@schoolId", schoolId);

                    try
                    {
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception of the type : " + e.ToString());
                    }
                }
            }
        }

        private static string educatorCheck(string id, string role)
        {
            // 1. Trainee Educator, 2. Educator, 3. Teaching staff, 4. Non-teaching staff, 5. Collaborator and 6. Student

            //if(role.Equals("Trainee Educator", StringComparison.InvariantCultureIgnoreCase) || 
            //    role.Equals("Educator", StringComparison.InvariantCultureIgnoreCase) || 
            //    role.Equals("Teaching staff", StringComparison.InvariantCultureIgnoreCase) || 
            //    role.Equals("Non-teaching staff", StringComparison.InvariantCultureIgnoreCase) ||
            //    role.Equals("Student", StringComparison.InvariantCultureIgnoreCase))

            // checks for ids of roles corresponding to one of the following
            // 1. 2001, 2. 2002, 3. 2003, 4. 2004, 5. 2005 and 6. 2006
            if (role.Equals("2001") || role.Equals("2002") || role.Equals("2003") || role.Equals("2004") || role.Equals("2005") || role.Equals("2006"))
            {
                // request sent to admin to allow access to the user on the platform
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("addToEducatorConfirmations", sqlConnection))
                    {
                        sqlConnection.Open();
                        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@id", id);
                        if (sqlCommand.ExecuteNonQuery() > 0)
                        {
                            // sql query executed successfully and the user is pending approval from the admin
                            Console.WriteLine("Query executed successfully");
                        }
                        else
                        {
                            Console.WriteLine("Query did not execute successfully");
                        }
                    }
                }
            }

            // if the user is Collaborator - he is granted student level access by default
            return "Student";
        }


        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    // ViewBag.Message = "User doesn't exist or lacks a confirmed Email Id!!";
                    ViewBag.Message = "Invalid";
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                //ViewBag.Message = "Please check your email to reset your password.";
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        
        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            ControllerContext.HttpContext.Session.RemoveAll();
            // Request a redirect to the external login provider
            returnUrl += "/";
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        //{
        //    var userId = await SignInManager.GetVerifiedUserIdAsync();
        //    if (userId == null)
        //    {
        //        return View("Error");
        //    }
        //    var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
        //    var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        //    return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        //}

        ////
        //// POST: /Account/SendCode
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SendCode(SendCodeViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View();
        //    }

        //    // Generate the token and send it
        //    if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
        //    {
        //        return View("Error");
        //    }
        //    return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        //}

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            ViewBag.Categories = CategoriesService.GetAllCategories().categories;

            ViewBag.UserRoles = context.Roles.Where(u => !u.Name.Contains("Admin")).ToList();
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            // fetches all the userroles from the database - student, educator
            ViewBag.UserRoles = context.Roles.Where(u => !u.Name.Contains("Admin")).ToList();
            ViewBag.Categories = CategoriesService.GetAllCategories().categories;
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider

                var email = "";
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info.Login.LoginProvider.Equals("Google") || info.Login.LoginProvider.Equals("Microsoft"))
                {
                    email = info.Email;
                }
                else if (info.Login.LoginProvider.Equals("Facebook"))
                {
                    email = info.DefaultUserName + "@facebook.com";
                }
                else if (info.Login.LoginProvider.Equals("Yahoo"))
                {
                    email = info.DefaultUserName + "@yahoo.com";
                }
                else
                {
                    email = info.DefaultUserName;
                }
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = email, Email = email, firstName = model.Firstname, lastName = model.Lastname, Birthdate = model.Birthdate };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        string role = educatorCheck(user.Id, model.UserRoles);
                        await this.UserManager.AddToRoleAsync(user.Id, role);

                        // update school, zone, category, role
                        updateSchoolZoneCategoryRole(user.Id, model.Zone, model.School, model.UserRoles, model.Category);
                        // updateSchoolandZone(user.Id, model.Zone, model.School);

                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOff()
        {
            //var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            //if (loginInfo.Login.LoginProvider.Equals("https://sts.windows.net/9c2ebe7b-ccbd-46bc-84ba-3289bba3b7c6/"))
            //{

            //    HttpContext.GetOwinContext().Authentication.SignOut(
            //       OpenIdConnectAuthenticationDefaults.AuthenticationType, Microsoft.Owin.Security.Cookies.CookieAuthenticationDefaults.AuthenticationType);
            //}
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        [HttpGet]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
            //context.Dispose();
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}