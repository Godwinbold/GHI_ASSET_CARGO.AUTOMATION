using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GHI_ASSET_CARGO.Core.Abstractions
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string recipientEmail, string subject, string body);
    }
}

