using GHI_ASSET_CARGO.Core.Abstractions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Infrastructure
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly IConfiguration _config;

        public CloudinaryService(IConfiguration configuration)
        {
            _config = configuration;
            var cloudinaryUrl = _config.GetSection("Cloudinary:Url").Value;
            
            if (string.IsNullOrEmpty(cloudinaryUrl))
                throw new InvalidOperationException("Cloudinary:Url configuration is missing or empty");

            _cloudinary = new Cloudinary(cloudinaryUrl);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? folder = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty", nameof(file));

            using (var stream = file.OpenReadStream())
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folder ?? "ghi-cargo",
                    UseFilename = true,
                    UniqueFilename = true,
                    Overwrite = false
                };

                // Handle different file types
                if (file.ContentType?.ToLowerInvariant() == "application/pdf")
                {
                    // For PDFs, use raw upload
                    var rawParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folder ?? "ghi-cargo",
                        UseFilename = true,
                        UniqueFilename = true,
                        Overwrite = false
                    };
                    var rawResult = await _cloudinary.UploadAsync(rawParams);
                    if (rawResult.Error != null)
                        throw new Exception($"Cloudinary upload failed: {rawResult.Error.Message}");
                    return rawResult.SecureUrl?.ToString() ?? rawResult.Url.ToString();
                }

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                    throw new Exception($"Cloudinary upload failed: {uploadResult.Error.Message}");

                return uploadResult.SecureUrl?.ToString() ?? uploadResult.Url.ToString();
            }
        }

        public async Task<bool> DeleteFileAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId))
                throw new ArgumentException("Public ID cannot be null or empty", nameof(publicId));

            var deleteParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deleteParams);

            return result.Result == "ok";
        }

        public string ExtractPublicIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty", nameof(url));

            try
            {
                // Extract public ID from URL
                // URL format: https://res.cloudinary.com/[cloud_name]/image/upload/[folder]/[public_id]
                var uri = new Uri(url);
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (segments.Length > 0)
                {
                    var fileName = segments[segments.Length - 1];
                    // Remove file extension
                    var publicId = Path.GetFileNameWithoutExtension(fileName);
                    return publicId;
                }

                throw new Exception("Unable to extract public ID from URL");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to extract public ID from URL: {ex.Message}", ex);
            }
        }
    }
}
