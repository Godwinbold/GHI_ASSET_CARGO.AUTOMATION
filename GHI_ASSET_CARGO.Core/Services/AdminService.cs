using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Admin;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GHI_ASSET_CARGO.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Result<PagedResult<UserListDto>>> GetUsers(int pageNumber = 1, int pageSize = 20, string? search = null)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search) || (u.IdNumber != null && u.IdNumber.Contains(search)));
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = new List<UserListDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                items.Add(new UserListDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    AirlineId = u.AirlineId,
                    IdNumber = u.IdNumber,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    Roles = roles
                });
            }

            var result = new PagedResult<UserListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };

            return Result.Success(result);
        }
    }
}
