using System;

namespace GHI_ASSET_CARGO.Core.Abstractions;

public interface IMailSenderService
{
    Task<bool> SendByPostMarkAppAsync(string message, string to, string subject, string? bcc = null);
    Task<bool> SendByPostMarkAppWithAttAsync(string message, string to, string subject, string attachmentContent);
}
