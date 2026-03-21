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
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<AppUser> userManager, IRepository repository, IJwtService jwtService,
            IConfiguration configuration, IUnitOfWork unitOfWork, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _repository = repository;
            _jwtService = jwtService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
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
                    AirlineId = registerAdminDto.AirlineId,
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
                var body =
                    @$"Hi {user.FirstName}, Please click the link <a href='{confirmationLink}'>here</a> to confirm your account's email";
                try
                {
                    var emailResult = await _emailService.SendEmailAsync(user.Email, "Confirm Email", body);

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
                    AirlineId = registerExecutiveDto.AirlineId,
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
                var body =
                    @$"Hi {user.FirstName}, Please click the link <a href='{confirmationLink}'>here</a> to confirm your account's email";
                try
                {
                    var emailResult = await _emailService.SendEmailAsync(user.Email, "Confirm Email", body);
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

        public async Task<Result<LoginResponseDto>> Login(LoginRequestDto loginUserDto, string airlineId)
        {
           
            var user = await _userManager.FindByEmailAsync(loginUserDto.Email);

            if (user is null)
                return new Error[] { new("Auth.Error", "email or password not correct") };

            var isValidUser = await _userManager.CheckPasswordAsync(user, loginUserDto.Password);

            if (!isValidUser)
                return new Error[] { new("Auth.Error", "email or password not correct") };

            if (!string.Equals(user.AirlineId, airlineId, StringComparison.OrdinalIgnoreCase))
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

            var emailBody = $"Hello {user.FirstName}, click this link to reset your password: {resetLink}.";

            var isSuccessful = await _emailService.SendEmailAsync(resetPasswordDto.Email, emailSubject, emailBody);
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
