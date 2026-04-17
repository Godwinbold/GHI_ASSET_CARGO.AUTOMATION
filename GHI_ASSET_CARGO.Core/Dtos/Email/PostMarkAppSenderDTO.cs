using System;
using System.Text.Json.Serialization;

namespace GHI_ASSET_CARGO.Core.Dtos.Email;

public class PostMarkAppSenderDTO
{
    [JsonPropertyName("From")]
    public string From { get; set; }

    [JsonPropertyName("To")]
    public string To { get; set; }

    [JsonPropertyName("Bcc")]
    public string Bcc { get; set; }

    [JsonPropertyName("Subject")]
    public string Subject { get; set; }

    [JsonPropertyName("TextBody")]
    public string TextBody { get; set; }

    [JsonPropertyName("HtmlBody")]
    public string HtmlBody { get; set; }

    [JsonPropertyName("MessageStream")]
    public string MessageStream { get; set; }

    [JsonPropertyName("Attachments")]
        public List<AttachmentDTO> Attachments { get; set; } = new();
    }

    public class AttachmentDTO
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Content")]
        public string Content { get; set; }

        [JsonPropertyName("ContentType")]
        public string ContentType { get; set; }
    }