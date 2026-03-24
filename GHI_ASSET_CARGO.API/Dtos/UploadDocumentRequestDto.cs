using Microsoft.AspNetCore.Http;

namespace GHI_ASSET_CARGO.API.Dtos
{
    public class UploadDocumentRequestDto
    {
        public IFormFile File { get; set; } = null!;
        public int UploadedByUserId { get; set; }
    }
}