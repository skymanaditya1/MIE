using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;
using System.IO;
using System.Data.SqlClient;
using MecMauritius.Services;
using MecMauritius.MecMauritiusCodeFirstModels;

namespace MecMauritius
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    /// 

    [Authorize(Roles = "Educator, Admin")]
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            var json = "";
            context.Response.ContentType = "application/json";
            string link = context.Request.Form["Link"].ToString();
            string grade = context.Request.Form["GradeType"].ToString();
            string subject = context.Request.Form["SubjectType"].ToString();
            string resourceTitle = context.Request.Form["ResourceTitle"].ToString();
            string resourceDescription = context.Request.Form["ResourceDescription"].ToString();
            string author = context.Request.Form["Author"].ToString();
            string emailId = System.Web.HttpContext.Current.User.Identity.Name.ToString();

            if (link.Length > 0)
            {
                FileUploadUtils.UpdateResourceDatabase(
                    "Link",
                    new Uri(link, UriKind.Absolute),
                    grade,
                    subject,
                    "Link",
                    resourceTitle,
                    resourceDescription,
                    author,
                    emailId,
                    new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/link.png", UriKind.Absolute)
                );
            }
            else
            {
                HttpFileCollection files = context.Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];
                    string directoryPath = context.Server.MapPath("~/Content/Resources");
                    bool exists = System.IO.Directory.Exists(directoryPath);
                    if (!exists)
                        System.IO.Directory.CreateDirectory(directoryPath);

                    var path = Path.Combine(directoryPath, file.FileName);

                    // Check for uploading of duplicate resources
                    if (FileUploadUtils.CheckFileDuplicate(file.FileName))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Cannot upload file. File already exists. Please change Filename to proceed." });
                        context.Response.StatusCode = 400;
                        context.Response.Write(json);
                        return;
                    }

                    // Client side validation for illegal file format upload
                    if (grade.Equals("Select Grade") || subject.Equals("Select Subject"))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Restricted File Format" });
                        context.Response.StatusCode = 400;
                        context.Response.Write(json);
                        return;
                    }
                    file.SaveAs(path);
                    string type = FileUploadUtils.GetResourceType(Path.GetExtension(path));
                    // Client side validation for illegal file format upload
                    if (type.Equals("Others"))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Restricted File Format" });
                        context.Response.StatusCode = 400;
                        context.Response.Write(json);
                        return;
                    }
                    //Upload file to cloud
                    CloudStorage cloudStorage = new CloudStorage();
                    Uri resourceURI = cloudStorage.UploadResourceToBlob(path, file.FileName);
                    //Upload thumbnail to cloud if Image/Video else get generic Icon Uri
                    Uri thumbnailURI = FileUploadUtils.GetThumbnailURI(path, type, cloudStorage);

                    //Put various kinds of docs under E-Books
                    //Note: Do not generalize type before generating thumbnailURI
                    if (type.Equals("E-Books") || type.Equals("Word") || type.Equals("Excel") || type.Equals("Powerpoint") || type.Equals("PDF")
                            || type.Equals("Epub") || type.Equals("Apk"))
                        type = "E-Books";

                    FileUploadUtils.UpdateResourceDatabase(file.FileName, resourceURI, grade, subject, type, resourceTitle, resourceDescription, author, emailId, thumbnailURI);
                }
            }
            json = new JavaScriptSerializer().Serialize(new { status = "success" });
            context.Response.Write(json);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}