using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IAuthService
    {
        Task<Result<string>> RegisterUser(RegisterRequestDto registerUserDto,string airlineId);
        Task<Result> InviteUser(InviteUserDto inviteUserDto);
        Task<Result> AcceptInvite(AcceptInviteDto acceptInviteDto);
        Task<Result> RegisterAdmin(AdminRegisterDTO registerAdminDto);
        Task<Result> RegisterExecutive(ExecutiveRegisterDTO registerExecutiveDto);
        Task<Result> AddUserToAirline(Guid userId, Guid airlineId);
        Task<Result> RemoveUserFromAirline(Guid userId, Guid airlineId);
        Task<Result<LoginResponseDto>> Login(LoginRequestDto loginUserDtox, string airlineId);
        Task<Result<LoginResponseDto>> GetCurrentLoggedInUser(string userId);
        Task<Result> ForgotPassword(ResetPasswordDto resetPasswordDto);
        Task<Result> ConfirmEmail(string email, string token);
        Task<Result> ChangePasswordAsync(ChangePasswordDto model);
        Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }
}
