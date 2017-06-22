using MecMauritius.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using System.Globalization;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Routing;
using System.Web.Script.Serialization;
using MecMauritius.Controllers;
using PagedList;
using PagedList.Mvc;
using System.Net.Mail;
using System.Net;

namespace MecMauritius.Controllers
{

    public class HomeController : Controller
    {
        private GradeDBContext GradeDb = new GradeDBContext();
        private CourseDBContext CourseDb = new CourseDBContext();
        private ResourceTypeDBContext ResourceTypeDb = new ResourceTypeDBContext();

        public string gradeDefault = "Select Grade", courseDefault = "Select Course", resourceTypeDefault = "Select Resource Type";

        // GET: Home
        [HttpGet]
        public ActionResult Index(int? page)
        {
            if (!Request.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            ViewBag.ResourceTypes = ResourceTypeDb.ResourceTypes.ToList();
            ViewBag.Grades = GradeDb.Grades.ToList();
            ViewBag.Courses = CourseDb.Courses.ToList();

            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ViewBag.Name = user.Name;
                ViewBag.displayUpload = "No";
                ViewBag.Admin = "No";

                if (IsAdmin())
                {
                    ViewBag.Admin = "Yes";
                }

                if (!IsOnlyStudent())
                {
                    ViewBag.displayUpload = "Yes";
                }
                return View();
            }
            else
            {
                ViewBag.Name = "Not Logged IN";
            }
            return View();
        }

        // Increments the download counter of the digital resource in the digitalresources table, 
        // params - resource_id
        public ActionResult IncrementDownloadCounter(string rID, string rURL)
        {
            // System.Diagnostics.Process.Start(rURL);
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {

                SqlCommand cmd = new SqlCommand("spUpdateDownloadCounter", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("rID", rID);
                connection.Open();
                cmd.ExecuteNonQuery();
                // System.Diagnostics.Process.Start(rURL);

                // return RedirectToAction("ResourceDisplay", new { ID = rID });
                return Redirect(rURL);
            }
        }

        // Function adds a review by a user for a resource 
        // Params, review is the text review, id is the resource id
        [HttpPost]
        public ActionResult AddReview(string review, string id)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                // Current time required or the Resource_Posted field in ResourceReviews
                DateTime currentTime = DateTime.Now;

                connection.Open();
                SqlCommand cmd = new SqlCommand("spAddtoResourceReviews", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@userid", User.Identity.GetUserId());
                cmd.Parameters.AddWithValue("@resourceid", id);
                cmd.Parameters.AddWithValue("@review", review);
                cmd.Parameters.AddWithValue("@currentTime", currentTime);
                cmd.ExecuteNonQuery(); // Inserts the review
            }
            return RedirectToAction("ResourceDisplay", new { ID = id });
        }
        
        // Form POST Method
        // Sends the Report Abuse Mail to the Admin
        [HttpPost]
        public ActionResult ResourceAbuseDescription()
        {
            // retrieve form details
            var resourceName = Request.Form["resourceName"];
            var resourceDescription = Request.Form["resourceDescription"];
            var resourceFeedback = Request.Form["resourceFeedback"];

            var fromAddress = "mecmauritius@gmail.com";
            var toAddress = "feedbackmec@mieonline.org";
            var fromPassword = "WaterMelon88";
            string subject = "Report Abuse : " + resourceName.ToString();
            string userEmail = User.Identity.GetUserName();
            string body = "Sent from User : " + User.Identity.GetUserName() + "\nIssue : " + resourceDescription + "\nFeedback : " + resourceFeedback;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress, fromPassword)
            };
            using(var message = new MailMessage(fromAddress, toAddress){
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }

            // The mail has been successfully delivered
            return RedirectToAction("Index");
        }

        // Form POST Method
        // Adds the Report Abuse Option to the Database
        [HttpPost]
        public ActionResult ResourceAbuseDescription1()
        {
            // Get the form information from the view
            var resourceName = Request.Form["resourceName"];
            var resourceDescription = Request.Form["resourceDescription"];
            var resourceFeedback = Request.Form["resourceFeedback"];
            // Update the information into the database
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spReportAbuse", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@SenderMail", User.Identity.GetUserName());
                    sqlCommand.Parameters.AddWithValue("@Title", resourceName);
                    sqlCommand.Parameters.AddWithValue("@Description", resourceDescription);
                    sqlCommand.Parameters.AddWithValue("@Feedback", resourceFeedback);
                    sqlConnection.Open();
                    if (sqlCommand.ExecuteNonQuery() <= 0)
                        return View("ErrorView");
                    else
                        return RedirectToAction("Index");
                }
            }
        }

        // Report abuse about the resource 
        // user information - Email
        // description - Resource Name, Description
        // Send a mail to the admin's mail ID
        public ActionResult ReportAbuse()
        {
            return View();
        }

        // Controller action to display resource details using the resourceID of the resource
        public ActionResult ResourceDisplay(string ID)
        {
            List<ResourceDisplay> displayResource = new List<ResourceDisplay>();
            List<ResourceReviews> reviewsResource = new List<ResourceReviews>();
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {

                SqlCommand cmd = new SqlCommand("spResourceFetchQuery", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", ID);
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        displayResource.Add(new ResourceDisplay
                        {
                            Resource_Thumbnail = reader.GetValue(0).ToString(),
                            Resource_Title = reader.GetValue(1).ToString(),
                            Resource_Grade = reader.GetValue(2).ToString(),
                            Resource_Subject = reader.GetValue(3).ToString(),
                            Ratings = reader.GetValue(4).ToString(),
                            Downloads = reader.GetValue(5).ToString(),
                            Download_URL = reader.GetValue(6).ToString(),
                            Description = reader.GetValue(7).ToString(),
                            Resource_ID = ID
                        });
                    }
                }

                // Add the user id of the user to ViewBag
                ViewBag.ResourceUser = User.Identity.GetUserId();

                cmd = new SqlCommand("spReviewFetchQuery", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", ID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int numberReviews = 0;
                    while (reader.Read())
                    {
                        numberReviews += 1;
                        reviewsResource.Add(new ResourceReviews
                        {
                            Name = reader.GetValue(0).ToString(),
                            Description = reader.GetValue(1).ToString(),
                            TimePosted = (DateTime)reader.GetValue(2),
                            Rating = (float)(float.Parse(reader.GetValue(3).ToString(), CultureInfo.InvariantCulture.NumberFormat) / 5.0),
                            User_ID = reader.GetValue(4).ToString()
                        });
                    }
                    ViewBag.ReviewNumbers = numberReviews;
                }
            }
            ViewBag.ResourcesDisplayed = displayResource;
            ViewBag.ResourceReviews = reviewsResource;
            return View();
        }

        // Method to get zones from categories
        [HttpGet]
        public JsonResult GetZonesFromCategories(string categoryid)
        {
            ZonesModel zonesModel = new ZonesModel();
            zonesModel.zones = new Collection<Zone>();
            zonesModel.zones.Add(new Zone
            {
                ID = "0",
                Name = "Select Zone"
            });
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetZonesFromCategories", sqlConnection))
                {
                    sqlConnection.Open();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@categoryid", categoryid);
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            zonesModel.zones.Add(new Zone
                            {
                                ID = reader.GetValue(0).ToString(),
                                Name = reader.GetValue(1).ToString()
                            });
                        }
                    }
                }
            }
            if (zonesModel.zones.Count > 0)
                zonesModel.Selected = 0;
            return Json(zonesModel.zones, JsonRequestBehavior.AllowGet);
        }

        // Method to fetch the roles from categories
        [HttpGet]
        public JsonResult GetRolesFromCategories(string categoryid)
        {
            UserRolesModel userRolesModel = new UserRolesModel();
            userRolesModel.roles = new Collection<UserRoles>();
            userRolesModel.roles.Add(new UserRoles
            {
                ID = "0",
                Name = "Select Role"
            });
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetRolesFromCategories", sqlConnection))
                {
                    sqlConnection.Open();
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@categoryid", categoryid);
                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userRolesModel.roles.Add(new UserRoles
                            {
                                ID = reader.GetValue(0).ToString(),
                                Name = reader.GetValue(1).ToString()
                            });
                        }
                    }
                }
            }

            if (userRolesModel.roles.Count > 0)
                userRolesModel.Selected = 0;

            return Json(userRolesModel.roles, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        // Method allows user to update a review
        public JsonResult UpdateResourceReview(string userid, string resourceid, string description)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spUpdateResourceReview", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userid", userid);
                    sqlCommand.Parameters.AddWithValue("@resourceid", resourceid);
                    sqlCommand.Parameters.AddWithValue("@description", description);
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
            return Json("Review updated successfully");
        }

        [HttpPost]
        // Method allows user to delete a review
        public JsonResult DeleteReview(string userid, string resourceid)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spDeleteReview", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@userid", userid);
                    sqlCommand.Parameters.AddWithValue("@resourceid", resourceid);
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
            return Json("Review deleted successfully");
        }

        // Method to submit ratings and reviews
        [HttpPost]
        public ActionResult RatingsReviewSubmission()
        {
            bool flag = true;
            // Before the review is submitted check if the review has already been submitted by the user for the resource
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {

                connection.Open();
                SqlCommand cmd = new SqlCommand("spRatingsReviewSubmission", connection);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@USER_ID", User.Identity.GetUserId());
                cmd.Parameters.AddWithValue("@Resource_ID", Request.Form["resourceId"].ToString());
                // int numberRows = (int)cmd.ExecuteScalar();
                if (cmd.ExecuteReader().HasRows)
                {
                    // display an alert message
                    flag = false;
                }

            }
            if (flag == true)
            {
                Response.ContentType = "text/plain";
                // context.Response.Write("Hello World");
                float Relevance = float.Parse(Request.Form["Relevance"], CultureInfo.InvariantCulture.NumberFormat);
                float Adaptability = float.Parse(Request.Form["Adaptability"], CultureInfo.InvariantCulture.NumberFormat);
                float Language = float.Parse(Request.Form["Language"], CultureInfo.InvariantCulture.NumberFormat);
                float Pedagogy = float.Parse(Request.Form["Pedagogy"], CultureInfo.InvariantCulture.NumberFormat);
                float Design = float.Parse(Request.Form["Design"], CultureInfo.InvariantCulture.NumberFormat);

                float total = Relevance + Adaptability + Language + Pedagogy + Design;

                String review = Request.Form["reviewText"].ToString();
                String id = Request.Form["resourceId"].ToString();
                float currRating = 0.0f, noOfRatings = 0.0f;
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("spFetchCurrentRating", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {

                            currRating = float.Parse(reader.GetValue(0).ToString(), CultureInfo.InvariantCulture.NumberFormat);
                            noOfRatings = float.Parse(reader.GetValue(1).ToString(), CultureInfo.InvariantCulture.NumberFormat);

                        }
                    }
                    noOfRatings++;
                    currRating = ((currRating * (noOfRatings - 1)) + total / 5) / noOfRatings;

                    string updatecurrentrating = @"UPDATE [dbo].[DigitalResources]
                                               SET [Ratings] = " + currRating +
                                                      ",[NoOfRatings] = " + noOfRatings +
                                                   "WHERE [ID]= " + id + ";";

                    cmd = new SqlCommand(updatecurrentrating, connection);
                    cmd.ExecuteNonQuery();
                }

                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
                {
                    // Current time required or the Resource_Posted field in ResourceReviews
                    DateTime currentTime = DateTime.UtcNow;
                    // string query = "INSERT INTO ResourceReviews (User_ID, Resource_ID, Resource_Description, Resource_Posted, Relevance, Adaptability, Language, Pedagogy, Design, Total_Rating) VALUES (@userid, @resourceid, @review, @currentTime, @relevance, @adaptability, @language, @pedagogy, @design, @total)";

                    connection.Open();
                    SqlCommand cmd = new SqlCommand("spUpdateUserResourceRatings", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@userid", System.Web.HttpContext.Current.User.Identity.GetUserId());
                    cmd.Parameters.AddWithValue("@resourceid", id);
                    cmd.Parameters.AddWithValue("@review", review);
                    cmd.Parameters.AddWithValue("@relevance", Relevance);
                    cmd.Parameters.AddWithValue("@currentTime", currentTime);
                    cmd.Parameters.AddWithValue("@adaptability", Adaptability);
                    cmd.Parameters.AddWithValue("@language", Language);
                    cmd.Parameters.AddWithValue("@pedagogy", Pedagogy);
                    cmd.Parameters.AddWithValue("@design", Design);
                    cmd.Parameters.AddWithValue("@total", total);
                    cmd.ExecuteNonQuery(); // Inserts the review
                }
                // return RedirectToAction("ResourceDisplay", "Home", new { ID = id });
            }

            else
            {
                // Display an error message - resource has already been reviewed
                return RedirectToAction("ReviewSubmitted", new { ID = Request.Form["resourceId"].ToString() });
            }

            return RedirectToAction("ResourceDisplay", "Home", new { ID = Request.Form["resourceID"].ToString() });
        }

        [HttpGet]
        // Method to get the schools for the zone
        public JsonResult GetSchoolsFromZone(string zoneid)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;

            SchoolModel schoolModel = new SchoolModel();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("spFetchSchools", connection))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@zone_id", zoneid);
                    connection.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    schoolModel.Schoollist = new Collection<School>();

                    schoolModel.Schoollist.Add(new School { Name = "Select School", Id = "0" });

                    while (reader.Read())
                    {
                        string name = string.Format(CultureInfo.InvariantCulture, reader.GetValue(1).ToString());
                        string id = string.Format(CultureInfo.InvariantCulture, reader.GetValue(0).ToString());

                        schoolModel.Schoollist.Add(new School { Name = name, Id = id });
                    }
                }
            }

            if (schoolModel.Schoollist.Count > 0)
                schoolModel.Selected = 0;

            return Json(schoolModel.Schoollist, JsonRequestBehavior.AllowGet) ;
        }

        // redirects the user to an error page, already submitted a review
        public ActionResult ReviewSubmitted(String id)
        {
            TempData["alertMessage"] = "Reviews cannot be submitted more than once. You have already submitted a review for this resource";
            ViewBag.ResID = id;
            // return RedirectToAction("ResourceDisplay", "Home", new { ID = Request.Form["resourceID"].ToString() });
            return View();
        }

        // Method to reduce the length of the resource description to 100 characters
        public string Chop(string text, int chopLength, string postfix = "...")
        {
            if (text == "" || text == null)
                return "No Description Provided";
            if (text == null || text.Length < chopLength)
                return text;
            else
                return text.Substring(0, chopLength - postfix.Length) + postfix;
        }

        // Method to reduce the length of the title to restrict it to 2 lines
        public string ChopTitle(string text, int chopLength, string postfix = "...")
        {
            if (text == "" || text == null)
                return "No Description Provided";
            if (text == null || text.Length < chopLength)
                return text;
            else
                return text.Substring(0, chopLength - postfix.Length) + postfix;
        }

        //[ValidateAntiForgeryToken]
        [HttpGet]
        public JsonResult GetSchools(string id)
        {
            SchoolModel schoolModel = SchoolService.GenerateSchools(id);

            return Json(schoolModel.Schoollist, JsonRequestBehavior.AllowGet);
        }

        public Boolean IsOnlyStudent()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var userStore = new UserStore<ApplicationUser>(context);
                var UserManager = new UserManager<ApplicationUser>(userStore);
                var s = UserManager.GetRoles(user.GetUserId());

                if (s.Contains("Educator") || s.Contains("Admin"))
                {
                    return false;
                }

                else
                {
                    return true;
                }
            }

            return true;
        }

        public Boolean IsEducator()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var userStore = new UserStore<ApplicationUser>(context);
                var UserManager = new UserManager<ApplicationUser>(userStore);
                var s = UserManager.GetRoles(user.GetUserId());

                if (s.Contains("Educator"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public Boolean IsAdmin()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = User.Identity;
                ApplicationDbContext context = new ApplicationDbContext();
                var userStore = new UserStore<ApplicationUser>(context);
                var UserManager = new UserManager<ApplicationUser>(userStore);
                var s = UserManager.GetRoles(user.GetUserId());

                if (s.Contains("Admin"))
                {

                    return true;
                }

                else
                {
                    return false;
                }
            }
            return true;
        }

        // Method allows user to delete resource
        [HttpPost]
        public JsonResult DeleteResource(string ID)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spDeleteResource", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@ID", ID);
                    sqlConnection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
            }
            return Json("Success"); // return code upon success
        }

        // Method allows user to edit resources
        public ActionResult EditResource(string ID)
        {
            ViewBag.Grades = GradeDb.Grades.ToList();
            ViewBag.Courses = CourseDb.Courses.ToList();
            List<ResourceEdit> resources = new List<ResourceEdit>();
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spEditResource", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@ID", ID);
                    sqlConnection.Open();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        resources.Add(new ResourceEdit
                        {
                            ResourceID = ID,
                            Grade = reader.GetValue(0).ToString(),
                            Subject = reader.GetValue(1).ToString(),
                            Description = reader.GetValue(2).ToString(),
                            Title = reader.GetValue(3).ToString(),
                            Thumbnail = reader.GetValue(4).ToString()
                        });
                    }
                }
            }
            ViewBag.EditedResource = resources;
            return View();
        }

        // Method allows user to update resources which needs editing
        // Method follows EditResource controller action
        public ActionResult UpdateResourceDetails()
        {
            bool fileUpload = false;
            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0) fileUpload = true;
            var fileName = "";
            var path = "";
            Uri uri;
            string url = "";

            CloudStorageAccount storageAccount = GetStorageAccount();
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("mecmauritius");

            if (fileUpload == true)
            {
                HttpPostedFileBase file = Request.Files[0];
                fileName = Path.GetFileName(file.FileName);
                var extension = Path.GetExtension(file.FileName);

                CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

                path = Path.Combine(Server.MapPath("~/Content/Resources"), fileName);
                file.SaveAs(path);

                using (var stream = System.IO.File.OpenRead(path))
                {
                    blob.UploadFromStream(stream);
                    uri = blob.Uri;
                    url = uri + "";
                }
            }

            else
            {
                url = Request.Form["url"];
            }

            if (!Request.Form["title"].ToString().Trim().Equals("")
                && !Request.Form["description"].ToString().Trim().Equals("")
                && !Request.Form["chosenGrade"].ToString().Trim().Equals("")
                && !Request.Form["chosenCourse"].ToString().Trim().Equals(""))
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
                {
                    using (SqlCommand sqlCommand = new SqlCommand("spUpdateResourceDetails", sqlConnection))
                    {
                        sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlCommand.Parameters.AddWithValue("@Title", Request.Form["title"]);
                        sqlCommand.Parameters.AddWithValue("@Description", Request.Form["description"]);
                        sqlCommand.Parameters.AddWithValue("@Grade", Request.Form["chosenGrade"]);
                        sqlCommand.Parameters.AddWithValue("@Course", Request.Form["chosenCourse"]);
                        sqlCommand.Parameters.AddWithValue("@Url", url);
                        sqlCommand.Parameters.AddWithValue("@ID", Request.Form["resourceID"]);
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                return Json("One or more fields are empty");
            }

            return RedirectToAction("Index", "Home");
        }

        // Method to fetch the resources when the Search Query is specified, 
        // STORED PROCEDURE NEEDS IMPLEMENTATION
        // Returns an empty page when the query returns 0 resources
        [HttpGet]
        public JsonResult QuerySearch(string queryName, string grade, string subject, string type, bool displayEducatorResources)
        {
            int resourcesReturned = 0;
            queryName = queryName ?? "";
            grade = grade ?? "";
            queryName = queryName ?? "";
            subject = subject ?? "";
            type = type ?? "";
            grade = grade.Equals("Select Grade") ? "" : grade;
            subject = subject.Equals("Select Course") ? "" : subject;
            type = type.Equals("Select Resource Type") ? "" : type;

            List<DigitalResources> models = new List<DigitalResources>();
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString;
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {

                string uploaderid = "";
                if (displayEducatorResources && IsEducator() && !IsAdmin())
                {
                    uploaderid = User.Identity.Name.ToString();
                }

                sqlConnection.Open();
                String[] tokens = queryName.Split(' ');
                string query = "";
                if (tokens.Length != 0)
                {
                    query = @"SELECT Title, Description, Thumbnail_URL, URL, Education, Grade, Subjects, Downloads, Ratings, ID " +
                              "FROM DigitalResources " +
                              "WHERE UploaderId LIKE '%" + uploaderid + "%'" +
                               " AND Grade LIKE '%" + grade + "%' and " +
                                    "Subjects LIKE '%" + subject + "%' and " +
                                    "Resource_Type LIKE '%" + type + "%' and " +
                                     "( Title LIKE '%" + tokens[0] + "%'";
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        query += " OR Title LIKE '%" + tokens[i] + "%' ";
                    }

                    query += " ) ORDER BY Downloads DESC, Ratings DESC ; ";
                }
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {

                    using (SqlDataReader reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            resourcesReturned += 1;
                            models.Add(new DigitalResources
                            {
                                ResourceTitle = ChopTitle(reader.GetValue(0).ToString(), 25),
                                ResourceDescription = Chop(reader.GetValue(1).ToString(), 100),
                                ResourceThumbnail = reader.GetValue(2).ToString(),
                                ResourceUrl = new Uri(reader.GetValue(3).ToString(), UriKind.Absolute),
                                ResourceEducation = reader.GetValue(4).ToString(),
                                ResourceGrades = reader.GetValue(5).ToString(),
                                ResourceSubject = reader.GetValue(6).ToString(),
                                Downloads = reader.GetValue(7).ToString(),
                                Ratings = reader.GetValue(8).ToString(),
                                ResourceID = reader.GetValue(9).ToString()
                            });
                        }
                    }
                }
            }
            // return an empty view if none of the resources are returned
            //if (resourcesReturned == 0)
            //{
            //    return Json(new { success = false, responseText = "Your query did not return any result" }, JsonRequestBehavior.AllowGet);
            //}
            //else 
            //{
            //    return Json(models, JsonRequestBehavior.AllowGet);
            //}
            if (resourcesReturned == 0)
            {
                return Json("Your query did not return any result.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(models, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        // Method allows userRoles to upload Resources
        // Required User Roles on the platform - Educator / Admin
        // Below method also prevents the upload of duplicate resources
        public ActionResult UploadResource()
        {
            ViewBag.ResourceTypes = ResourceTypeDb.ResourceTypes.ToList();
            ViewBag.Grades = GradeDb.Grades.ToList();
            ViewBag.Courses = CourseDb.Courses.ToList();
            var json = "";
            Response.ContentType = "application/json";
            if (Request.Files.Count > 0)
            {
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFileBase file = files[i];
                    var fileName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(fileName);

                    bool thumbnailUpload = false;
                    bool exists = System.IO.Directory.Exists(Server.MapPath("~/Content/Resources"));
                    if (!exists)
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/Resources"));

                    var path = Path.Combine(Server.MapPath("~/Content/Resources"), fileName);

                    // Get Info from form
                    string grade = Request.Form["GradeType"].ToString();
                    string subject = Request.Form["SubjectType"].ToString();
                    string resourceTitle = Request.Form["ResourceTitle"].ToString();
                    string resourceDescription = Request.Form["ResourceDescription"].ToString();
                    string emailId = User.Identity.Name.ToString();

                    string type = getResourceType(extension);
                    if (type.Equals("E-Books") || type.Equals("Word") || type.Equals("Excel") || type.Equals("Powerpoint") || type.Equals("PDF"))
                        type = "E-Books";

                    if (grade.Equals("Select Grade") || subject.Equals("Select Subject") || type.Equals("Others"))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Restricted File Format" });
                        Response.StatusCode = 400;
                        Response.Write(json);
                        return View(json);
                    }

                    file.SaveAs(path);

                    // Generate thumbnail for the pdf file
                    string outputPath = Path.Combine(Server.MapPath("~/Content/Resources"), fileName.Split('.')[0] + "_thumbnail.jpg");
                    string outputFileName = fileName.Split('.')[0] + "_thumbnail.jpg";

                    // Thumbnail for the Image file
                    // Thumbnail is the same as the image
                    if (getResourceType(extension).Equals("Image"))
                    {
                        // outputPath = Path.Combine(Server.MapPath("~/Content/Resources"), fileName);
                        outputPath = path;
                        outputFileName = fileName;
                    }

                    // Thumbnail for Video Files
                    else if (getResourceType(extension).Equals("Video"))
                    {
                        generateVideoThumbs(path, outputPath);
                        thumbnailUpload = true;
                    }

                    GenerateResource(path, fileName, grade, subject, type,
                        resourceTitle, resourceDescription, emailId, outputPath, outputFileName, thumbnailUpload);

                }
            }

            json = new JavaScriptSerializer().Serialize(new { status = "success" });
            Response.Write(json);

            return View("Index");
        }

        // Method generates the URL for thumbnail and resource after uploading them to Azure Cloud Storage
        // Also updates the details in the database
        public void GenerateResource(string path, string fileName,
            string grade, string subject, string type, string resourceTitle, string resourceDescription, string emailId, string outputPath, string outputFileName, bool thumbnailUpload)
        {
            Uri resourceURI = UploadResourceToBlob(path, fileName);
            Uri thumbnailURI = null;
            var extension = Path.GetExtension(fileName);
            if (thumbnailUpload)
                thumbnailURI = UploadResourceToBlob(outputPath, outputFileName);
            else
            {
                if (getResourceType(extension).Equals("Compressed"))
                {
                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/compressed.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Word"))
                {

                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/word.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Excel"))
                {

                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/excel.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("PDF"))
                {

                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/pdf.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Powerpoint"))
                {

                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/powerpoint.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Others"))
                {

                    thumbnailURI = new Uri("https://mlxstorage.blob.core.windows.net/mecmauritius/file.png", UriKind.Absolute);

                }
            }
            UpdateResourceDatabase(fileName, resourceURI, grade, subject, type, resourceTitle, resourceDescription, emailId, thumbnailURI);
        }

        // Updates the database with information of resource
        public static void UpdateResourceDatabase(string resource, Uri url,
            string grade, string subject, string type, string resourceTitle, string resourceDescription, string emailId, Uri thumbnailUri)
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spUploadResourcesNew", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@Name", resource);
                    sqlCommand.Parameters.AddWithValue("@Education", "Primary");
                    sqlCommand.Parameters.AddWithValue("@Grade", grade);
                    sqlCommand.Parameters.AddWithValue("@Subjects", subject);
                    sqlCommand.Parameters.AddWithValue("@Resource_Type", type);
                    sqlCommand.Parameters.AddWithValue("@URL", url + "");
                    sqlCommand.Parameters.AddWithValue("@Description", resourceDescription);
                    sqlCommand.Parameters.AddWithValue("@Title", resourceTitle);
                    sqlCommand.Parameters.AddWithValue("@UploaderId", emailId);
                    sqlCommand.Parameters.AddWithValue("@Thumbnail_URL", thumbnailUri + "");

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

        // Method uploads resource to Azure Cloud Storage
        public Uri UploadResourceToBlob(string fileName, string resource)
        {
            CloudStorageAccount cloudStorageAccount = GetStorageAccount();
            CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("mecmauritius");
            container.CreateIfNotExists();
            var permissions = new BlobContainerPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
            container.SetPermissions(permissions);
            CloudBlockBlob blob = container.GetBlockBlobReference(resource);
            using (var filestream = System.IO.File.OpenRead(fileName))
            {
                blob.UploadFromStream(filestream);
                return blob.Uri;
            }
        }

        // Method to generate the thumbnail from videos
        private static void generateVideoThumbs(string inputVideoPath, string outputImagePath)
        {
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(inputVideoPath, outputImagePath, 5);
        }

        // Returns file extension
        private static string getResourceType(string extension)
        {
            extension = extension.ToLower(CultureInfo.InvariantCulture);
            String[] imageList = new[] { ".bmp", ".dds", ".gif", ".jpg", ".png", ".psd", ".pspimage", ".tga", ".thm", ".tif", ".tiff", ".yuv", ".jpeg" };
            String[] videoList = new[] { ".3g2", ".3gp", ".asf", ".avi", ".flv", ".m4v", ".mov", ".mp4", ".mpg", ".rm", ".swf", ".vob", ".wmv" };
            String[] docList = new[] { ".log", ".msg", ".odt", ".pages", ".rtf", ".txt", ".tex", ".wpd", ".wps", ".pps", ".xlr", };
            String[] pdfList = new[] { ".pdf" };
            String[] wordList = new[] { ".doc", ".docx" };
            String[] excelList = new[] { ".xls", ".xlsx" };
            String[] pptList = new[] { ".ppt", ".pptx" };
            String[] compressedList = new[] { ".7z", ".cbr", ".deb", ".gz", ".pkg", ".rar", ".rpm", ".sitx", ".tar.gz", ".zip", ".zipx" };
            String[] flashList = new[] { ".flv", ".swf" };
            String[] htmlList = new[] { ".htm", ".html", ".asp", ".aspx", ".js", ".jsp", ".xhtml" };
            String[] audioList = new[] { ".aif", ".aiff", ".iff", ".m3u", ".m4a", ".mid", ".mp3", ".mpa", ".wav", ".wma" };

            if ((imageList).Contains(extension))
                return "Image";
            else if ((videoList).Contains(extension))
                return "Video";
            else if ((docList).Contains(extension))
                return "E-Books";
            else if ((wordList).Contains(extension))
                return "Word";
            else if ((excelList).Contains(extension))
                return "Excel";
            else if ((pptList).Contains(extension))
                return "Powerpoint";
            else if ((pdfList).Contains(extension))
                return "PDF";
            else if ((compressedList).Contains(extension))
                return "Compressed";
            else if ((flashList).Contains(extension))
                return "Flash";
            else if ((audioList).Contains(extension))
                return "Audio";
            else
                return "Others";
        }

        /// <summary>
        /// Fetches the StorageAccount Settings
        /// </summary>
        /// <returns></returns>
        public static CloudStorageAccount GetStorageAccount()
        {
            CloudStorageAccount cloudStorageAccount = null;

            string azureStorageAccountName = ConfigurationManager.AppSettings["MLXStorage"].
                Split(new char[] { ';' })[1].Substring("AccountName=".Length);
            //string azureStorageAccountKey = ConfigurationManager.AppSettings["MLXStorage"].
            //  Split(new char[] { ';' })[2].Substring("AccountKey=".Length);
            string blobEndpointURL = string.Format(CultureInfo.InvariantCulture, "https://{0}.blob.core.windows.net/", azureStorageAccountName);
            string queueEndpointURL = string.Format(CultureInfo.InvariantCulture, "https://{0}.queue.core.windows.net/", azureStorageAccountName);
            string tableEndpointURL = string.Format(CultureInfo.InvariantCulture, "https://{0}.table.core.windows.net/", azureStorageAccountName);
            cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["MLXStorage"]);
            return cloudStorageAccount;
        }
    }
}