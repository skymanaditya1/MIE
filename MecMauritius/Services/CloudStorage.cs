using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MecMauritius.Services
{
    public class CloudStorage
    {
        CloudStorageAccount CloudStorageAccount;
        public CloudStorage()
        {
            CloudStorageAccount cloudStorageAccount = null;
            string azureStorageAccountName = ConfigurationManager.AppSettings["MLXStorage"].
                Split(new char[] { ';' })[1].Substring("AccountName=".Length);
            cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["MLXStorage"]);
            this.CloudStorageAccount = cloudStorageAccount;
        }
        public Uri UploadResourceToBlob(string fileName, string resource)
        {
            CloudBlobClient blobClient = CloudStorageAccount.CreateCloudBlobClient();
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
    }
}