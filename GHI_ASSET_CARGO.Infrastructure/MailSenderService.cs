using System.Net.Mime;
using System.Text;
using System.Text.Json;
using GHI_ASSET_CARGO.Core.Abstractions;
using GHI_ASSET_CARGO.Core.Dtos.Email;
using GHI_ASSET_CARGO.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace GHI_ASSET_CARGO.Infrastructure;

public class MailSenderService : IMailSenderService
{
   private readonly HttpClient  _httpClient;
    private readonly PostMarkOptions _config;

    private readonly ILogger<MailSenderService> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly JsonSerializerOptions _jsonSnakeSerializerOptions;
    


    public MailSenderService(IHttpClientFactory factory, PostMarkOptions config, ILogger<MailSenderService> logger)
    {
        _httpClient = factory.CreateClient();
        _httpClient.BaseAddress = new Uri(config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Clear();
        _config = config;
        _logger = logger;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _jsonSnakeSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    }

public async Task<bool> SendByPostMarkAppAsync(string message, string to, string subject, string? bcc = null)
{
    try
    {
        var model = new PostMarkAppSenderDTO
        {
            From = "info@codebasehackers.com",
            To = to,
            Bcc = bcc,
            MessageStream = "outbound",
            HtmlBody = message,
            Subject = subject
        };

        var jsonBody = JsonSerializer.Serialize(model, _jsonSnakeSerializerOptions);
        var content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);
            var headers = new Dictionary<string, string> { { "X-Postmark-Server-Token", _config.PostMarkToken } };

            foreach (var header in headers)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            _logger.LogInformation("Sending email message to {To}: {Subject}", model.To, subject);

        var cancellationToken = default(CancellationToken);
        var response = await _httpClient.PostAsync(_config.BaseUrl, content, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("PostMark API error: {Status} - {Body}", response.StatusCode, responseContent);
                return false;
        }

        var result = JsonSerializer.Deserialize<PostMarkAppResponseDTO>(responseContent, _jsonSerializerOptions);
        _logger.LogInformation("Email with subject: {subject} sent successfully to {email}", subject, to);
            return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error while sending email message to {To}", to);
         return false;
    }
}
    

    public async Task<bool> SendByPostMarkAppWithAttAsync(string message, string to, string subject, string attachmentContent)
    {
        try
        {
            var model = new PostMarkAppSenderDTO
            {
                From = "info@codebasehackers.com",
                To = to,
                MessageStream = "outbound",
                HtmlBody = message,
                Subject = subject,
                Attachments = new List<AttachmentDTO>
                {
                    new()
                    {
                        Name = "Report.pdf",
                        Content = attachmentContent,
                        ContentType = "application/octet-stream"
                    }
                }
            };

            _logger.LogInformation("Sending to postmark with attachment");
            var jsonBody = JsonSerializer.Serialize(model, _jsonSnakeSerializerOptions);
            var content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);

            var cancellationToken = default(CancellationToken);
            var response = await _httpClient.PostAsync(_config.BaseUrl, content, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("PostMark API error: {Status} - {Body}", response.StatusCode, responseContent);
                return false;
            }

            _logger.LogInformation("Email with subject: {subject} sent successfully to {email}", subject, to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending email with attachment to {To}", to);
            return false;
        }
    }
}
