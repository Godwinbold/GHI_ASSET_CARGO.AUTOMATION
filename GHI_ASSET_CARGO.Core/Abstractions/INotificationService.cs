using System;

namespace GHI_ASSET_CARGO.Core.Abstractions;

public interface INotificationService
{
	Task<bool> InviteAsync(string email, string organizationName, string invitationLink);
	Task<bool> SendTemplateAsync(string email, string subject, string templateName, IDictionary<string, string> replacements);

	// Task<bool> SendResetPasswordAsync(string email, string otp);
	// Task <bool> SendAdminNotificationAsync(string email,string amount, string transRef, string createdDate, string organizationName);
}
