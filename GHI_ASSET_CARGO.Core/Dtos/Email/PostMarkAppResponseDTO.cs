using System;
using System.Text.Json.Serialization;

namespace GHI_ASSET_CARGO.Core.Dtos.Email;

public class PostMarkAppResponseDTO
{
    [JsonPropertyName("To")]
    public string To { get; set; }

    [JsonPropertyName("SubmittedAt")]
    public DateTimeOffset SubmittedAt { get; set; }

    [JsonPropertyName("MessageID")]
    public Guid MessageId { get; set; }

    [JsonPropertyName("ErrorCode")]
    public long ErrorCode { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }
}
