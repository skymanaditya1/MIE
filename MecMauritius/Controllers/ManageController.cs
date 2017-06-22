using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MecMauritius.Models;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using MecMauritius.Controllers;
using System.Collections.Generic;

namespace MecMauritius.Controllers
{
    [Authorize]
    //[ValidateAntiForgeryToken]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: /Manage/Index
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            ViewBag.Categories = CategoriesService.GetAllCategories().categories;
            // ZoneModel zoneModels = ZonesService.GetAllZones();
            // ViewBag.ZoneType = ZonesService.GetAllZones().Zoneitems;

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            int zone, school;
            zone = school = -1;

            // get the current value of category, userrole, zone and school
            GetCategoryRoleZoneSchool(user.Id);

            var model = new ModifyModel
            {
                Firstname = user.firstName,
                Lastname = user.lastName,
                Birthdate = user.Birthdate
            };
            return View(model);
        }

        public void GetCategoryRoleZoneSchool(string userid)
        {
            ViewBag.CurrentCategory = "No Category Chosen";
            ViewBag.CurrentRole = "No Role Chosen";
            ViewBag.CurrentZone = "No Zone Chosen";
            ViewBag.CurrentSchool = "No School Chosen";

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetAllUserInformation", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userId", userid);
                    sqlConnection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ViewBag.CurrentCategory = reader.GetValue(0).ToString();
                            ViewBag.CurrentRole = reader.GetValue(1).ToString();
                            ViewBag.CurrentZone = reader.GetValue(2).ToString();
                            ViewBag.CurrentSchool = reader.GetValue(3).ToString();
                        }
                    }
                }
            }
        }

        // GET: /Manage/Index
        [HttpPost]
        public async Task<ActionResult> Modify(ModifyModel model)
        {
            ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            user.firstName = model.Firstname;
            user.lastName = model.Lastname;
            user.Birthdate = model.Birthdate;

            IdentityResult result = await UserManager.UpdateAsync(user);
            // modifySchoolandZone(user.Id, model.Zone, model.School);
            modifyCategoryRoleZoneSchool(user.Id, model.Zone, model.School, model.Category, model.UserRoles);

            return RedirectToAction("Index", "Manage");
        }

        // method to update category, role, zone and school
        public void modifyCategoryRoleZoneSchool(string userid, int zoneid, int schoolid, string categoryid, string userroleid)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spUpdateUserInformation", sqlConnection))
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

        // GET: /Manage/Delete
        [HttpGet]
        public async Task<ActionResult> Delete()
        {
            var context = new ApplicationDbContext();

            var AutheticationManager = HttpContext.GetOwinContext().Authentication;
            AuthenticationManager.SignOut();

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            UserManager.Delete(user);

            return RedirectToAction("Index", "Home");
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

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}