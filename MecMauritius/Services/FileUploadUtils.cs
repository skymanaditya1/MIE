using SwfDotNet.IO;
using SwfDotNet.IO.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;

namespace MecMauritius.Services
{
    public class FileUploadUtils
    {
        public static Uri GetThumbnailURI(string path, string type, CloudStorage cloudStorage)
        {
            // Generate the thumbnail file path
            string outputFileName = Path.GetFileNameWithoutExtension(path) + "_thumbnail.jpg";
            string outputPath = Path.Combine(Path.GetDirectoryName(path), outputFileName);

            Uri thumbnailURI;

            switch (type) {
                case "Image":
                    outputFileName = Path.GetFileNameWithoutExtension(path);
                    outputPath = Path.Combine(Path.GetDirectoryName(path), outputFileName);
                    thumbnailURI = cloudStorage.UploadResourceToBlob(outputPath, outputFileName);
                    break;
                case "Video":
                    generateVideoThumbs(path, outputPath);
                    thumbnailURI = cloudStorage.UploadResourceToBlob(outputPath, outputFileName);
                    break;
                case "Compressed":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/compressed.png", UriKind.Absolute);
                    break;
                case "Word":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/word.png", UriKind.Absolute);
                    break;
                case "Epub":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/epub.png", UriKind.Absolute);
                    break;
                case "Apk":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/apk.png", UriKind.Absolute);
                    break;
                case "Excel":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/excel.png", UriKind.Absolute);
                    break;
                case "PDF":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/pdf.png", UriKind.Absolute);
                    break;
                case "Powerpoint":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/powerpoint.png", UriKind.Absolute);
                    break;
                case "Others":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/file.png", UriKind.Absolute);
                    break;
                case "Ubz":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/sankore.png", UriKind.Absolute);
                    break;
                case "Flash":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/flash-resource.png", UriKind.Absolute);
                    break;
                case "Audio":
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/audio.png", UriKind.Absolute);
                    break;
                default:
                    thumbnailURI = new Uri("https://miemecstorage.blob.core.windows.net/mecmauritius/file.png", UriKind.Absolute);
                    break;
            }          
            return thumbnailURI;
        }


        // Method to generate the thumbnail from videos
        public static void generateVideoThumbs(string inputVideoPath, string outputImagePath)
        {
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            ffMpeg.GetVideoThumbnail(inputVideoPath, outputImagePath, 5);
        }
        // Method for generating thumbnails of flash files
        //public static string GenerateThumbnailsFromSwf(string flashPath, string flashImagePath)
        //{
        //    SwfReader swfReader = new SwfReader(flashPath);
        //    try
        //    {
        //        Swf swf = swfReader.ReadSwf();
        //        // Browse through the swf tags list and generate the appropriate tag
        //        IEnumerator tagsEnumerator = swf.Tags.GetEnumerator();
        //        while (tagsEnumerator.MoveNext())
        //        {
        //            BaseTag baseTag = (BaseTag)tagsEnumerator.Current;
        //            if (baseTag is DefineBitsJpeg2Tag)
        //            {
        //                ((DefineBitsJpeg2Tag)baseTag).DecompileToFile(flashImagePath);
        //                Image image = ((DefineBitsJpeg2Tag)baseTag).DecompileToImage();
        //                return "Image";
        //            }
        //            else if (baseTag is DefineSoundTag)
        //            {
        //                // Extract a sound track from the flash file
        //                DefineSoundTag soundTag = (DefineSoundTag)baseTag;
        //                // Extract to an audio file
        //                if (soundTag.SoundFormat == SoundCodec.MP3)
        //                    soundTag.DecompileToFile(flashImagePath.Substring(0, flashImagePath.LastIndexOf('.') + 1) + "mp3");
        //                else
        //                    soundTag.DecompileToFile(flashImagePath.Substring(0, flashImagePath.LastIndexOf('.') + 1) + "wav");
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        // if an exception occurs then continue with the flow
        //        return "Audio";
        //    }
        //    return "Audio";
        //}
        public static string GetResourceType(string extension)
        {
            extension = extension.ToLower(CultureInfo.InvariantCulture);
            String[] imageList = new[] { ".bmp", ".dds", ".gif", ".jpg", ".png", ".psd", ".pspimage", ".tga", ".thm", ".tif", ".tiff", ".yuv", ".jpeg" };
            String[] videoList = new[] { ".3gp", ".asf", ".avi", ".flv", ".mov", ".mp4", ".mpg", ".vob", ".wmv" };
            String[] audioList = new[] { ".mp3", ".wav", ".aiff", ".wmv", ".flac", ".m3u", ".wma", ".aac", ".ogg" };
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
            else if ((audioList).Contains(extension))
                return "Audio";
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
        // Method checks for file duplicacy by matching uploading file name with file names present on server
        public static bool CheckFileDuplicate(string fileName)
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
        public static void UpdateResourceDatabase(string resource, Uri url,
            string grade, string subject, string type, string resourceTitle, string resourceDescription, string author  , string emailId, Uri thumbnailUri)
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
                    sqlCommand.Parameters.AddWithValue("@Author", author);
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
    }
}