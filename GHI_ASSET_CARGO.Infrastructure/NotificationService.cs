using System;
using System.Collections.Generic;
using System.IO;
using GHI_ASSET_CARGO.Core.Abstractions;
using Microsoft.Extensions.Hosting;

namespace GHI_ASSET_CARGO.Infrastructure;

public class NotificationService(IHostEnvironment hostEnvironment, IMailSenderService mailSenderService) : INotificationService
{
	private readonly IHostEnvironment _hostEnvironment = hostEnvironment;
	private readonly IMailSenderService _mailSenderService = mailSenderService;

    public async Task<bool> InviteAsync(string email, string organizationName, string role, string? airlineName, string invitationLink)
	{
		var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "organization_invite_email.html");
		var htmlMessage = File.ReadAllText(fullPath);
		htmlMessage = htmlMessage.Replace("{{organizationName}}", organizationName);
		htmlMessage = htmlMessage.Replace("{{role}}", role);
		htmlMessage = htmlMessage.Replace("{{airlineName}}", string.IsNullOrWhiteSpace(airlineName) ? "the platform" : airlineName);
		htmlMessage = htmlMessage.Replace("{{invitationLink}}", invitationLink);
		var subject = string.IsNullOrWhiteSpace(airlineName)
		    ? $"GHI Asset Cargo Invitation: {role}"
		    : $"GHI Asset Cargo Invitation to {airlineName}";
		var isMailSent =  await _mailSenderService.SendByPostMarkAppAsync(htmlMessage, email, subject);
		return isMailSent;
	}

	public async Task<bool> SendTemplateAsync(string email, string subject, string templateName, IDictionary<string, string> replacements)
	{
		var fullPath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", templateName);
		var htmlMessage = File.ReadAllText(fullPath);

		foreach (var replacement in replacements)
		{
			htmlMessage = htmlMessage.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
		}

		return await _mailSenderService.SendByPostMarkAppAsync(htmlMessage, email, subject);
	}
}
