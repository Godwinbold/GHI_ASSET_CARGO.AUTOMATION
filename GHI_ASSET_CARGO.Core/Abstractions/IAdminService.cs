using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Admin;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IAdminService
    {
        Task<Result<PagedResult<UserListDto>>> GetUsers(int pageNumber = 1, int pageSize = 20, string? search = null);
        Task<Result<object>> DeleteUserAsync(string userId, string? adminUserId = null, string? adminEmail = null, string? adminName = null, string? ipAddress = null);
    }
}
