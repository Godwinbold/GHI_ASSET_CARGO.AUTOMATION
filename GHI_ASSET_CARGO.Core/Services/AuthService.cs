using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos;
using GHI_ASSET_CARGO.Core.Dtos.Auth;
using GHI_ASSET_CARGO.Domain.Constants;
using GHI_ASSET_CARGO.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Web;



namespace GHI_ASSET_CARGO.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<AppUser> userManager, IRepository repository, IJwtService jwtService,
            IConfiguration configuration, IUnitOfWork unitOfWork, INotificationService notificationService, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _repository = repository;
            _jwtService = jwtService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result<string>> RegisterUser(RegisterRequestDto registerUserDto, string airlineId)
        {
            try
            {
                var airline = await _repository.FindById<Airline>(Guid.Parse(airlineId));
                if (airline == null)
                    return new Error[] { new("Airline.NotFound", "Airline not found. You can only register for a valid airline portal.") };

                var user = new AppUser
                {
                    FirstName = registerUserDto.FirstName,
                    LastName = registerUserDto.LastName,
                    MiddleName = registerUserDto.MiddleName,
                    Email = registerUserDto.Email,
                    PhoneNumber = registerUserDto.PhoneNumber,
                    UserName = registerUserDto.Email,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UpdatedDate = DateTimeOffset.UtcNow,
                    IdNumber = registerUserDto.IdNumber,
                    AirlineId = airlineId
                };


                var result = await _userManager.CreateAsync(user, registerUserDto.Password);

                if (!result.Succeeded)
                    return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                result = await _userManager.AddToRoleAsync(user, RolesConstant.User);
                if (!result.Succeeded)
                    return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                return Result<string>.Success("User registered successfully");
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to create executive, {ex.Message}");
                return new Error[] { new("Error", "Failed to create user") };
            }
        }

        public async Task<Result> RegisterAdmin(AdminRegisterDTO registerAdminDto)
        {
            try
            {
                var emailExist = await _userManager.FindByEmailAsync(registerAdminDto.Email);

                if (emailExist != null)
                    return new Error[] { new("Registration.Error", "email already exist") };

                var user = new AppUser
                {
                    FirstName = registerAdminDto.FirstName,
                    MiddleName = registerAdminDto.MiddleName,
                    LastName = registerAdminDto.LastName,
                    Email = registerAdminDto.Email,
                    PhoneNumber = registerAdminDto.PhoneNumber,
                    UserName = registerAdminDto.Email,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UpdatedDate = DateTimeOffset.UtcNow,
                    AirlineId = null,
                    IdNumber = registerAdminDto.IdNumber,

                };

                var result = await _userManager.CreateAsync(user, registerAdminDto.Password);
                if (!result.Succeeded)
                    return (result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray());

                result = await _userManager.AddToRoleAsync(user, RolesConstant.Admin);
                if (!result.Succeeded)
                    return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                var confirmEmailUrl = _configuration["ConfirmEmailUrl"];
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedEmail = HttpUtility.UrlEncode(user.Email);
                var encodedToken = HttpUtility.UrlEncode(token);
                var confirmationLink = $"{confirmEmailUrl}?email={encodedEmail}&token={encodedToken}";

                var templateData = new Dictionary<string, string>
                {
                    ["firstName"] = user.FirstName,
                    ["confirmationLink"] = confirmationLink
                };

                try
                {
                    var emailResult = await _notificationService.SendTemplateAsync(user.Email, "Confirm Email", "admin_confirmation_email.html", templateData);
                    if (!emailResult)
                    {
                        _logger.LogInformation($">>>>>>Sending of Email to {registerAdminDto.Email} failed");
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError($">>>>>>Sending of Email to {registerAdminDto.Email} failed, {ex.Message}");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create executive, {ex.Message}");
                return new Error[] { new("Error", "Failed to create admin user") };
            }

        }

        public async Task<Result> RegisterExecutive(ExecutiveRegisterDTO registerExecutiveDto)
        {
            try
            {
                var emailExist = await _userManager.FindByEmailAsync(registerExecutiveDto.Email);

                if (emailExist != null)
                    return new Error[] { new("Registration.Error", "email already exist") };

                var user = new AppUser
                {
                    FirstName = registerExecutiveDto.FirstName,
                    MiddleName = registerExecutiveDto.MiddleName,
                    LastName = registerExecutiveDto.LastName,
                    Email = registerExecutiveDto.Email,
                    PhoneNumber = registerExecutiveDto.PhoneNumber,
                    UserName = registerExecutiveDto.Email,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UpdatedDate = DateTimeOffset.UtcNow,
                    AirlineId = null,
                    IdNumber = registerExecutiveDto.IdNumber,

                };

                var result = await _userManager.CreateAsync(user, registerExecutiveDto.Password);
                if (!result.Succeeded)
                    return (result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray());

                result = await _userManager.AddToRoleAsync(user, RolesConstant.Executive);
                if (!result.Succeeded)
                    return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                var confirmEmailUrl = _configuration["ConfirmEmailUrl"];
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedEmail = HttpUtility.UrlEncode(user.Email);
                var encodedToken = HttpUtility.UrlEncode(token);
                var confirmationLink = $"{confirmEmailUrl}?email={encodedEmail}&token={encodedToken}";

                var organizationName = string.Empty;
                if (Guid.TryParse(registerExecutiveDto.AirlineId, out var airlineId))
                {
                    var airline = await _repository.FindById<Airline>(airlineId);
                    organizationName = airline?.AirlineName ?? string.Empty;
                }

                var templateData = new Dictionary<string, string>
                {
                    ["firstName"] = user.FirstName,
                    ["organizationName"] = string.IsNullOrWhiteSpace(organizationName) ? "GHI Asset Cargo" : organizationName,
                    ["confirmationLink"] = confirmationLink
                };

                try
                {
                    var emailResult = await _notificationService.SendTemplateAsync(user.Email, "Confirm Email", "executive_confirmation_email.html", templateData);
                    if (!emailResult)
                    {
                        _logger.LogInformation($">>>>>>Sending of Email to {registerExecutiveDto.Email} failed");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new Error[] { new("Error", "Failed to create executive user") };
            }
        }

        public async Task<Result> InviteUser(InviteUserDto inviteUserDto)
        {
            try
            {
                if (inviteUserDto == null)
                    return new Error[] { new("Invitation.Error", "Invalid invitation request") };

                if (string.IsNullOrWhiteSpace(inviteUserDto.Email) || !new EmailAddressAttribute().IsValid(inviteUserDto.Email))
                    return new Error[] { new("Invitation.Error", "A valid email is required") };

                var normalizedRole = inviteUserDto.Role.Trim().ToUpperInvariant();
                var validRoles = new[] { RolesConstant.User, RolesConstant.Admin, RolesConstant.Executive };
                if (!validRoles.Contains(normalizedRole))
                    return new Error[] { new("Invitation.Error", "Invalid role. Allowed roles are USER, ADMIN, EXECUTIVE") };

                // For Admin and Executive, AirlineId is not required
                if (normalizedRole == RolesConstant.User && string.IsNullOrWhiteSpace(inviteUserDto.AirlineId))
                    return new Error[] { new("Invitation.Error", "Airline is required for USER role") };

                Airline? airline = null;
                if (!string.IsNullOrWhiteSpace(inviteUserDto.AirlineId))
                {
                    if (!Guid.TryParse(inviteUserDto.AirlineId, out var airlineId))
                        return new Error[] { new("Invitation.Error", "Invalid airline identifier") };

                    airline = await _repository.FindById<Airline>(airlineId);
                    if (airline == null)
                        return new Error[] { new("Airline.NotFound", "Airline not found") };
                }

                var user = new AppUser
                {
                    FirstName = string.Empty,
                    MiddleName = string.Empty,
                    LastName = string.Empty,
                    Email = inviteUserDto.Email,
                    PhoneNumber = string.Empty,
                    UserName = inviteUserDto.Email,
                    CreatedDate = DateTimeOffset.UtcNow,
                    UpdatedDate = DateTimeOffset.UtcNow,
                    AirlineId = inviteUserDto.AirlineId,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    return createResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
                if (!roleResult.Succeeded)
                    return roleResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                var inviteUrl = _configuration["AcceptInviteUrl"];
                if (string.IsNullOrWhiteSpace(inviteUrl))
                    return new Error[] { new("Configuration.Error", "AcceptInviteUrl is not configured") };

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedEmail = HttpUtility.UrlEncode(user.Email);
                var encodedToken = HttpUtility.UrlEncode(token);
                var encodedRole = HttpUtility.UrlEncode(normalizedRole);
                var acceptLink = $"{inviteUrl}?email={encodedEmail}&token={encodedToken}&role={encodedRole}";
                if (!string.IsNullOrWhiteSpace(inviteUserDto.AirlineId))
                {
                    var encodedAirline = HttpUtility.UrlEncode(inviteUserDto.AirlineId);
                    acceptLink += $"&airlineId={encodedAirline}";
                }

                bool invitationSent = false;
                try
                {
                    invitationSent = await _notificationService.InviteAsync(
                        user.Email,
                        airline?.AirlineName ?? "GHI Asset Cargo",
                        normalizedRole,
                        airline?.AirlineName,
                        acceptLink);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send invitation email to {Email}", inviteUserDto.Email);
                    invitationSent = false;
                }

                if (!invitationSent)
                {
                    await _userManager.DeleteAsync(user);
                    return new Error[] { new("Invitation.Error", "Failed to send invitation email") };
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invite user");
                return new Error[] { new("Error", "Failed to invite user") };
            }
        }

        public async Task<Result> AddUserToAirline(Guid userId, Guid airlineId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new Error[] { new("User.NotFound", "User not found") };

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(RolesConstant.User))
                return new Error[] { new("User.RoleInvalid", "Only USER accounts can be assigned airline memberships") };

            var airline = await _repository.FindById<Airline>(airlineId);
            if (airline == null)
                return new Error[] { new("Airline.NotFound", "Airline not found") };

            user.AirlineId = AddAirlineId(user.AirlineId, airlineId.ToString());
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

            return Result.Success();
        }

        public async Task<Result> RemoveUserFromAirline(Guid userId, Guid airlineId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return new Error[] { new("User.NotFound", "User not found") };

            var userRoles = await _userManager.GetRolesAsync(user);
            if (!userRoles.Contains(RolesConstant.User))
                return new Error[] { new("User.RoleInvalid", "Only USER accounts can have airline memberships removed") };

            var airline = await _repository.FindById<Airline>(airlineId);
            if (airline == null)
                return new Error[] { new("Airline.NotFound", "Airline not found") };

            user.AirlineId = RemoveAirlineId(user.AirlineId, airlineId.ToString());
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return updateResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

            return Result.Success();
        }

        private static bool UserHasAirline(AppUser user, string airlineId)
        {
            var airlineIds = ParseAirlineIds(user.AirlineId);
            return airlineIds.Contains(airlineId, StringComparer.OrdinalIgnoreCase);
        }

        private static List<string> ParseAirlineIds(string? airlineIds)
        {
            if (string.IsNullOrWhiteSpace(airlineIds))
                return new List<string>();

            return airlineIds
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string AddAirlineId(string? existing, string airlineId)
        {
            var airlineIds = ParseAirlineIds(existing);
            if (!airlineIds.Contains(airlineId, StringComparer.OrdinalIgnoreCase))
                airlineIds.Add(airlineId);

            return string.Join(";", airlineIds);
        }

        private static string RemoveAirlineId(string? existing, string airlineId)
        {
            var airlineIds = ParseAirlineIds(existing);
            airlineIds.RemoveAll(x => string.Equals(x, airlineId, StringComparison.OrdinalIgnoreCase));
            return string.Join(";", airlineIds);
        }

        private async Task<bool> IsAdminOrExecutive(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Contains(RolesConstant.Admin) || roles.Contains(RolesConstant.Executive);
        }

        public async Task<Result> AcceptInvite(AcceptInviteDto acceptInviteDto)
        {
            try
            {
                if (acceptInviteDto == null)
                    return new Error[] { new("Invitation.Error", "Invalid request") };

                if (!new EmailAddressAttribute().IsValid(acceptInviteDto.Email))
                    return new Error[] { new("Invitation.Error", "A valid email is required") };

                if (acceptInviteDto.Password != acceptInviteDto.ConfirmPassword)
                    return new Error[] { new("Invitation.Error", "Password and confirm password must match") };

                var user = await _userManager.FindByEmailAsync(acceptInviteDto.Email);
                if (user == null)
                    return new Error[] { new("Invitation.Error", "No invitation exists for this email") };

                if (await _userManager.HasPasswordAsync(user))
                    return new Error[] { new("Invitation.Error", "This invitation has already been accepted") };

                if (!string.IsNullOrWhiteSpace(acceptInviteDto.AirlineId) &&
                    !UserHasAirline(user, acceptInviteDto.AirlineId))
                {
                    return new Error[] { new("Invitation.Error", "Airline information does not match the invitation") };
                }

                var resetResult = await _userManager.ResetPasswordAsync(user, acceptInviteDto.Token, acceptInviteDto.Password);
                if (!resetResult.Succeeded)
                    return resetResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                user.FirstName = acceptInviteDto.FirstName;
                user.LastName = acceptInviteDto.LastName;
                user.MiddleName = acceptInviteDto.MiddleName;
                user.PhoneNumber = acceptInviteDto.PhoneNumber;
                user.IdNumber = acceptInviteDto.IdNumber;
                user.UpdatedDate = DateTimeOffset.UtcNow;
                user.EmailConfirmed = true;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    return updateResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to accept invitation");
                return new Error[] { new("Error", "Failed to complete invitation") };
            }
        }

        public async Task<Result<LoginResponseDto>> Login(LoginRequestDto loginUserDto, string airlineId)
        {
           
            var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

            if (user is null)
                return new Error[] { new("Auth.Error", "email or password not correct") };

            var isValidUser = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);

            if (!isValidUser)
                return new Error[] { new("Auth.Error", "email or password not correct") };

if (!UserHasAirline(user, airlineId) && !await IsAdminOrExecutive(user))
                return new Error[] { new("Auth.AirlineMismatch", "You do not have access to this airline portal. Please use your airline's login page.") };
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            var response = new LoginResponseDto
            {
                Token = token,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                AirlineId = user.AirlineId,
                Roles = roles
            };

            return Result.Success(response);

        }




        public async Task<Result<LoginResponseDto>> GetCurrentLoggedInUser(string userId)
        {

            if (string.IsNullOrWhiteSpace(userId))
                return new Error[] { new("Auth.Error", "Could not resolve the current user.") };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new Error[] { new("Auth.Error", "User does not exist.") };

            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginResponseDto
            {
                Token = string.Empty,
                ExpiresAt = DateTimeOffset.MinValue,
                UserId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                AirlineId = user.AirlineId,
                Roles = roles
            };

            return response;
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user is null)
                return new Error[] { new("Auth.Error", "No user found with the provided email") };

            var resetPasswordResult =
                await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (!resetPasswordResult.Succeeded)
                return resetPasswordResult.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

            return Result.Success();
        }

        public async Task<Result> ForgotPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user == null)
                return new Error[] { new("Auth.Error", "No user found with the provided email") };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink =
                $"{_configuration["ResetPasswordUrl"]}?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(token)}";

            const string emailSubject = "Your New Password";

            var templateData = new Dictionary<string, string>
            {
                ["firstName"] = string.IsNullOrWhiteSpace(user.FirstName) ? "User" : user.FirstName,
                ["resetLink"] = resetLink
            };

            var isSuccessful = await _notificationService.SendTemplateAsync(resetPasswordDto.Email, emailSubject, "reset_password_email.html", templateData);
            if (!isSuccessful)
                return new Error[] { new("Auth.Error", "Error occured while sending reset password email") };

            return Result.Success();
        }

        public async Task<Result> ConfirmEmail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user is null)
                return new Error[] { new("Auth.Error", "User not found") };

            var confirmEmailResult = await _userManager.ConfirmEmailAsync(user, token);

            if (!confirmEmailResult.Succeeded)
            {
                return Result.Failure(confirmEmailResult.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
            }

            user.EmailConfirmed = true;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                return Result.Failure(updateResult.Errors.Select(e => new Error(e.Code, e.Description)).ToArray());
            }

            return Result.Success();
        }

        public async Task<Result> ChangePasswordAsync(ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return new Error[] { new("Auth.Error", "email not correct") };

            if (!await _userManager.CheckPasswordAsync(user, model.OldPassword))
                return new Error[] { new("Auth.Error", "password not correct") };

            if (model.NewPassword != model.ConfirmPassword)
                return new Error[] { new("Auth.Error", "Newpassword and Confirmpassword must match") };

            var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
                return result.Errors.Select(error => new Error(error.Code, error.Description)).ToArray();

            return Result.Success();

        }


    }
}
