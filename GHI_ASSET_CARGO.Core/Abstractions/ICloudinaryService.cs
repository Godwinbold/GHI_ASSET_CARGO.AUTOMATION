using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface ICloudinaryService
    {
        /// <summary>
        /// Upload a file to Cloudinary
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="folder">Optional folder path in Cloudinary</param>
        /// <returns>The public URL of the uploaded file</returns>
        Task<string> UploadFileAsync(IFormFile file, string? folder = null);

        /// <summary>
        /// Delete a file from Cloudinary
        /// </summary>
        /// <param name="publicId">The public ID of the file to delete</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteFileAsync(string publicId);

        /// <summary>
        /// Get the public ID from a Cloudinary URL
        /// </summary>
        /// <param name="url">The Cloudinary URL</param>
        /// <returns>The public ID</returns>
        string ExtractPublicIdFromUrl(string url);
    }
}
