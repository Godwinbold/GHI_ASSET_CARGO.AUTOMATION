using System;

namespace GHI_ASSET_CARGO.Domain.Options;

public class PostMarkOptions
    {
        public const string SectionName = "PostMark";

        public string BaseUrl { get; set; } = "https://api.postmarkapp.com/email";
        public string PostMarkToken { get; set; }
    }
