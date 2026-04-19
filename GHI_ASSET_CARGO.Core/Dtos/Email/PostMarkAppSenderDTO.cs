using System;
using System.Text.Json.Serialization;

namespace GHI_ASSET_CARGO.Core.Dtos.Email;

public class PostMarkAppSenderDTO
{
    [JsonPropertyName("From")]
    public required string From { get; set; }

    [JsonPropertyName("To")]
    public required string To { get; set; }

    [JsonPropertyName("Bcc")]
    public string? Bcc { get; set; }

    [JsonPropertyName("Subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("TextBody")]
    public required string TextBody { get; set; }

    [JsonPropertyName("HtmlBody")]
    public required string HtmlBody { get; set; }

    [JsonPropertyName("MessageStream")]
    public required string MessageStream { get; set; }

    [JsonPropertyName("Attachments")]
    public List<AttachmentDTO> Attachments { get; set; } = new();
}

public class AttachmentDTO
{
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    [JsonPropertyName("Content")]
    public required string Content { get; set; }

    [JsonPropertyName("ContentType")]
    public required string ContentType { get; set; }
}