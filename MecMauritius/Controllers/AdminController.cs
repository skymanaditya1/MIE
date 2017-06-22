using MecMauritius.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Net;
using System.IO;

namespace MecMauritius.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        [HttpGet]
        // Display the resources uploaded on the platform
        public ActionResult ResourcesUploaded()
        {
            List<ResourcesUploadedAdmin> resources = new List<ResourcesUploadedAdmin>();
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetDigitalResourcesAdmin", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlConnection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resources.Add(new ResourcesUploadedAdmin
                            {
                                Title = reader.GetValue(0).ToString(),
                                Description = reader.GetValue(1).ToString(),
                                UploaderName = reader.GetValue(2).ToString(),
                                UploaderEmail = reader.GetValue(3).ToString(),
                                Education = reader.GetValue(4).ToString(),
                                Grade = reader.GetValue(5).ToString(),
                                Subject = reader.GetValue(6).ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.UploadedResources = resources;
            return View();
        }

        [HttpGet]
        public static Collection<User> GetUserList(int mode)
        {
            Collection<User> users = new Collection<User>();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand command = new SqlCommand("spGetUserList", sqlConnection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@mode", mode);

                    sqlConnection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User u = new User();
                            u.Email=reader["Email"].ToString();
                            u.Name = reader["Name"].ToString();
                            u.Id=reader["Id"].ToString();
                            String p = reader["Denied"].ToString();
                            u.Zone = reader["Zone"].ToString();
                            u.School = reader["School"].ToString();

                            switch (p)
                            {
                                case "True": u.Permission=1; break;
                                case "False": u.Permission = 0; break;
                                default: u.Permission = -1; break;
                            }
                            users.Add(u);
                        }
                    }

                }
            }
            return users;
        }

        // GET: Admin
        [HttpGet]
        public ActionResult Index()
        {

            Collection<User> users = GetUserList(0);

            ViewBag.Users = users;

            return View();
        }

        [HttpGet]
        public ActionResult Approved()
        {
            Collection<User> users = GetUserList(1);
            ViewBag.Users = users;

            return View();
        }

        [HttpPost]
        public void sendUserApprovalMail(string toAddress, int approval)
        {
            // user request accepted
            if (approval == 0)
            {
                var fromEmailAddress = "feedbackmec@mieonline.org";
                var fromEmailPassword = "virtualcampus8217";
                // var toEmailAddress = User.Identity.GetUserName();
                string subject = "Virtual Campus Member request successfully approved.";
                string body = "Your request to become a member on the MIE Virtual Campus platform has been approved by the admin. " + 
                " You can now upload resources on the platform.";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword)
                };

                using (var message = new MailMessage(fromEmailAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }
            }

            // user denied request
            if (approval == 0)
            {

            }
        }

        [HttpGet]
        public Collection<SiteAdmins> GetAllSiteAdmins()
        {
            Collection<SiteAdmins> siteAdmins = new Collection<SiteAdmins>();

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetAllSiteAdmins", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlConnection.Open();
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            SiteAdmins siteAdmin = new SiteAdmins();
                            siteAdmin.ID = reader.GetValue(0).ToString();
                            siteAdmin.FirstName = reader.GetValue(1).ToString();
                            siteAdmin.LastName = reader.GetValue(2).ToString();
                            siteAdmin.Email = reader.GetValue(3).ToString();
                            siteAdmins.Add(siteAdmin);
                        }
                    }
                }
            }

            return siteAdmins;
        }

        [HttpGet]
        // Method to remove site admin
        public JsonResult RemoveSiteAdmin(string userid)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spRemoveSiteAdmin", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userid", userid);
                    sqlConnection.Open();
                    if (sqlCommand.ExecuteNonQuery() > 0)
                    {
                        Console.WriteLine("SQL Query ran successfully");
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Console.WriteLine("Some error occurred while executing query");
                        return Json("Failure", JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        // Method to add site admin
        public void AddSiteAdminCSV(string emailAddress)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spAddSiteAdmin", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@email", emailAddress);
                    sqlConnection.Open();
                    if (sqlCommand.ExecuteNonQuery() > 0)
                    {

                    }
                    else
                    {

                    }
                }
            }
        }

        // Method to add a site admin
        [HttpGet]
        public JsonResult AddSiteAdmin(string emailAddress)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spAddSiteAdmin", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@email", emailAddress);
                    sqlConnection.Open();
                    if (sqlCommand.ExecuteNonQuery() > 0)
                    {
                        // Console.WriteLine("SQL Query ran successfully");
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Failure", JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpPost]
        // Method to add the site admin through the CSV file
        public ActionResult AddAdminCSV()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/AdminUsers"), fileName);
                    file.SaveAs(path);

                    // Add all the email addresses in the CSV as admin accounts
                    AddAsAdminCSV(path);
                }
            }
            return RedirectToAction("ManageSiteAdmins");
        }

        // Method to add all email addresses as admins in the CSV file
        public void AddAsAdminCSV(string path)
        {
            List<string> adminEmails = new List<string>();
            using (var reader = new StreamReader(System.IO.File.OpenRead(path)))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    adminEmails.Add(line.Split()[0]);
                }
            }
            foreach (string adminEmail in adminEmails)
            {
                AddSiteAdminCSV(adminEmail);
            }
        }

        // View to Manage Site Admins
        public ActionResult ManageSiteAdmins()
        {
            ViewBag.SiteAdmins = GetAllSiteAdmins();
            return View();
        }

        // get the email address attached with the userId
        public string GetEmailFromUser(string id)
        {
            string email = "";
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                string query = "SELECT Email FROM AspNetUsers WHERE Id = '" + id + "';";
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlConnection.Open();
                    email = sqlCommand.ExecuteScalar().ToString();
                }
            }
            return email;
        }

        [HttpGet]
        public JsonResult UpdateEducatorApprovals(string userId, int approval)
        {
            // send mail if the user request has been approved or rejected
            sendUserApprovalMail(GetEmailFromUser(userId), approval);

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("alterEducatorConfirmations", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@uId", userId);
                    sqlCommand.Parameters.AddWithValue("@app", approval);

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

             ApplicationDbContext context = new ApplicationDbContext();

             var userStore = new UserStore<ApplicationUser>(context);
             var UserManager = new UserManager<ApplicationUser>(userStore);

            if (approval == 0)
            {                
                UserManager.AddToRoleAsync(userId, "Educator");
            }
            else{
                //If rights denied or revoked
                UserManager.RemoveFromRoleAsync(userId, "Educator");
            }
            return Json("Updated!", JsonRequestBehavior.AllowGet);
        }

    }
}