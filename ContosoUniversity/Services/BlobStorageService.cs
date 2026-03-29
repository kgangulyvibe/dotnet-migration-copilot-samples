using System;
using System.Configuration;
using System.IO;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ContosoUniversity.Services
{
    public class BlobStorageService : IDisposable
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService()
        {
            var endpoint = ConfigurationManager.AppSettings["AzureBlobStorageEndpoint"];
            var containerName = ConfigurationManager.AppSettings["AzureBlobStorageContainerName"] ?? "teaching-materials";

            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException(
                    "Azure Blob Storage endpoint is not configured. " +
                    "Set the 'AzureBlobStorageEndpoint' app setting in Web.config.");
            }

            var blobServiceClient = new BlobServiceClient(
                new Uri(endpoint),
                new DefaultAzureCredential());

            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            // PublicAccessType.Blob allows anonymous read access to blobs, matching the original
            // behavior where images were served directly from the web server's ~/Uploads/ directory.
            // For stricter access control, use PublicAccessType.None with SAS token-based URLs.
            _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        }

        /// <summary>
        /// Uploads a file stream to Azure Blob Storage and returns the blob URL.
        /// </summary>
        public string UploadBlob(Stream fileStream, string blobName, string contentType)
        {
            var blobClient = _containerClient.GetBlobClient(blobName);

            var blobHttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            // Upload with overwrite enabled and proper content type
            blobClient.Upload(fileStream, new BlobUploadOptions { HttpHeaders = blobHttpHeaders });

            return blobClient.Uri.ToString();
        }

        /// <summary>
        /// Deletes a blob from Azure Blob Storage by its URL or blob name.
        /// </summary>
        public bool DeleteBlob(string blobUrlOrName)
        {
            var blobName = ExtractBlobName(blobUrlOrName);
            if (string.IsNullOrEmpty(blobName))
            {
                return false;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = blobClient.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
            return response.Value;
        }

        /// <summary>
        /// Extracts the blob name from a full blob URL or returns the input if it's already a blob name.
        /// </summary>
        private string ExtractBlobName(string blobUrlOrName)
        {
            if (string.IsNullOrEmpty(blobUrlOrName))
            {
                return null;
            }

            // If it's a full URL, extract the blob name
            if (Uri.TryCreate(blobUrlOrName, UriKind.Absolute, out var uri))
            {
                // URL format: https://account.blob.core.windows.net/container/blobname
                // Path segments: /, container/, blobname
                var segments = uri.Segments;
                if (segments.Length >= 3)
                {
                    // Join all segments after the container name
                    return string.Join("", segments, 2, segments.Length - 2).TrimStart('/');
                }
            }

            // If it's a local path (legacy), extract just the filename
            if (blobUrlOrName.StartsWith("~/"))
            {
                return Path.GetFileName(blobUrlOrName);
            }

            return blobUrlOrName;
        }

        /// <summary>
        /// Gets the content type based on file extension.
        /// </summary>
        public static string GetContentType(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                default:
                    return "application/octet-stream";
            }
        }

        public void Dispose()
        {
            // BlobContainerClient does not require explicit disposal
        }
    }
}
