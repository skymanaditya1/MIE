using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using System.Globalization;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Data.SqlClient;
using SwfDotNet.IO;
using System.Collections;
using SwfDotNet.IO.Tags;
using System.Drawing;
//using Ghostscript;
//using GhostscriptSharp;
//using Microsoft.Office.Interop.Word;

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

            if (context.Request.Files.Count > 0)
            {
                HttpFileCollection files = context.Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];

                    var fileName = Path.GetFileName(file.FileName);
                    var extension = Path.GetExtension(fileName);

                    //flag if thumbnail upload is required
                    bool thumbnailUpload = false;
                    bool exists = System.IO.Directory.Exists(context.Server.MapPath("~/Content/Resources"));

                    if (!exists)
                        System.IO.Directory.CreateDirectory(context.Server.MapPath("~/Content/Resources"));

                    var path = Path.Combine(context.Server.MapPath("~/Content/Resources"), fileName);

                    string grade = context.Request.Form["GradeType"].ToString();
                    string subject = context.Request.Form["SubjectType"].ToString();
                    string resourceTitle = context.Request.Form["ResourceTitle"].ToString();
                    string resourceDescription = context.Request.Form["ResourceDescription"].ToString();
                    string emailId = System.Web.HttpContext.Current.User.Identity.Name.ToString();

                    string type = getResourceType(extension);
                    if (type.Equals("E-Books") || type.Equals("Word") || type.Equals("Excel") || type.Equals("Powerpoint") || type.Equals("PDF")
                            || type.Equals("Epub") || type.Equals("Apk"))
                        type = "E-Books";
                    
                    // Check for uploading of duplicate resources
                    if (checkFileDuplicate(fileName))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Cannot upload file. File already exists. Please change Filename to proceed." });
                        context.Response.StatusCode = 400;
                        context.Response.Write(json);
                        return;
                    }

                    // Client side validation for illegal file format upload
                    if (grade.Equals("Select Grade") || subject.Equals("Select Subject") || type.Equals("Others"))
                    {
                        json = new JavaScriptSerializer().Serialize(new { error = "Restricted File Format" });
                        context.Response.StatusCode = 400;
                        context.Response.Write(json);
                        return;
                    }

                    file.SaveAs(path);

                    // Generate the thumbnail for the pdf file
                    string outputPath = Path.Combine(context.Server.MapPath("~/Content/Resources"), fileName.Split('.')[0] + "_thumbnail.jpg");
                    string outputFileName = fileName.Split('.')[0] + "_thumbnail.jpg";

                    // Thumbnails for Image files
                    if (getResourceType(extension).Equals("Image"))
                    {
                        // override the existing path with the corresponding file
                        outputPath = Path.Combine(context.Server.MapPath("~/Content/Resources"), fileName);
                        outputFileName = fileName;
                        thumbnailUpload = true;
                    }

                    // Thumbnails for Video Files
                    else if (getResourceType(extension).Equals("Video"))
                    {
                        generateVideoThumbs(path, outputPath);
                        thumbnailUpload = true;
                    }

                    // Thumbnails for Flash Files
                    else if (getResourceType(extension).Equals("Flash"))
                    {
                        string tag = GenerateThumbnailsFromSwf(path, outputPath);
                        if (tag.Equals("Audio"))
                        {
                            thumbnailUpload = false;
                        }
                        else if (tag.Equals("Image"))
                        {
                            thumbnailUpload = true;
                        }
                        else if (tag.Equals("Flash not supported"))
                        {
                            thumbnailUpload = false;
                        }
                    }

                    GenerateResource(path, fileName,
                        grade, subject, type, resourceTitle, resourceDescription, emailId, outputPath, outputFileName, thumbnailUpload);
                }
            }

            json = new JavaScriptSerializer().Serialize(new { status = "success" });
            context.Response.Write(json);
        }

        // Method for generating thumbnails of flash files
        public static string GenerateThumbnailsFromSwf(string flashPath, string flashImagePath)
        {
            SwfReader swfReader = new SwfReader(flashPath);
            try { 
                Swf swf = swfReader.ReadSwf();
                // Browse through the swf tags list and generate the appropriate tag
                IEnumerator tagsEnumerator = swf.Tags.GetEnumerator();
                while (tagsEnumerator.MoveNext())
                {
                    BaseTag baseTag = (BaseTag)tagsEnumerator.Current;
                    if (baseTag is DefineBitsJpeg2Tag)
                    {
                        ((DefineBitsJpeg2Tag)baseTag).DecompileToFile(flashImagePath);
                        Image image = ((DefineBitsJpeg2Tag)baseTag).DecompileToImage();
                        return "Image";
                    }
                    else if (baseTag is DefineSoundTag)
                    {
                        // Extract a sound track from the flash file
                        DefineSoundTag soundTag = (DefineSoundTag)baseTag;
                        // Extract to an audio file
                        if (soundTag.SoundFormat == SoundCodec.MP3)
                            soundTag.DecompileToFile(flashImagePath.Substring(0, flashImagePath.LastIndexOf('.') + 1) + "mp3");
                        else
                            soundTag.DecompileToFile(flashImagePath.Substring(0, flashImagePath.LastIndexOf('.') + 1) + "wav");
                    }
                }
            }
            catch (Exception e)
            {
                // if an exception occurs then continue with the flow
                return "Audio";
            }

            return "Audio";
        }

        // Method checks for file duplicacy by matching uploading file name with file names present on server
        public bool checkFileDuplicate(string fileName)
        {
            int rowsReturned = 0;
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DBConnectionString"].ConnectionString))
            {
                using (SqlCommand sqlCommand = new SqlCommand("spGetFileCount", sqlConnection))
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@Name", fileName);
                    sqlConnection.Open();
                    rowsReturned = (int)sqlCommand.ExecuteScalar(); // returns the number of rows with matching names
                }
            }

            return rowsReturned > 0 ? true : false;
        }

        // Method to generate the thumbnail from videos
        private static void generateVideoThumbs(string inputVideoPath, string outputImagePath)
        {
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(inputVideoPath, outputImagePath, 5);
        }

        public void GenerateResource(string path, string fileName,
            string grade, string subject, string type, string resourceTitle, string resourceDescription, string emailId, string outputPath, string outputFileName, bool thumbnailUpload)
        {
            Uri resourceURI = UploadResourceToBlob(path, fileName);
            Uri thumbnailURI=null;
            var extension = Path.GetExtension(fileName);
            if (thumbnailUpload)
                thumbnailURI = UploadResourceToBlob(outputPath, outputFileName);
            else
            {
                if (getResourceType(extension).Equals("Compressed"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/compressed.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Word"))
                {

                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/word.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Epub"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/epub.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Apk"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/apk.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Excel"))
                {

                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/excel.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("PDF"))
                {

                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/pdf.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Powerpoint"))
                {

                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/powerpoint.png", UriKind.Absolute);

                }
                else if (getResourceType(extension).Equals("Others"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/file.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Ubz"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/sankore.png", UriKind.Absolute);
                }
                else if (getResourceType(extension).Equals("Flash"))
                {
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/flash-resource.png", UriKind.Absolute);
                }
            }

            UpdateResourceDatabase(fileName, resourceURI, grade, subject, type, resourceTitle, resourceDescription, emailId, thumbnailURI);
        }

        private static string getResourceType(string extension)
        {
            extension = extension.ToLower(CultureInfo.InvariantCulture);
            String[] imageList = new[] { ".bmp", ".dds", ".gif", ".jpg", ".png", ".psd", ".pspimage", ".tga", ".thm", ".tif", ".tiff", ".yuv", ".jpeg" };
            String[] videoList = new[] {".3gp", ".asf", ".avi", ".flv", ".mov", ".mp4", ".mpg", ".vob", ".wmv" };
            String[] docList = new[] { ".log", ".msg", ".odt", ".pages", ".rtf", ".txt", ".tex", ".wpd", ".wps", ".pps", ".xlr", };
            String[] pdfList = new[] { ".pdf" };
            String[] apkList = new[] { ".apk" };
            String[] epubList = new[] { ".epub" };
            String[] wordList = new[] { ".doc", ".docx" };
            String[] excelList = new[] { ".xls", ".xlsx" };
            String[] pptList = new[] { ".ppt", ".pptx" };
            String[] compressedList = new[] { ".7z", ".cbr", ".deb", ".gz", ".pkg", ".rar", ".rpm", ".sitx", ".tar.gz", ".zip", ".zipx" };
            String[] sankoreList = new[] { ".ubz" };
            String[] flashList = new[] { ".swf" };

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
            else if ((sankoreList).Contains(extension))
                return "Ubz";
            else if ((flashList).Contains(extension))
                return "Flash";
            else if ((apkList).Contains(extension))
                return "Apk";
            else if ((epubList).Contains(extension))
                return "Epub";
            else
                return "Others";
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

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