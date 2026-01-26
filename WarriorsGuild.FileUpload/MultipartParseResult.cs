using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace WarriorsGuild.Models
{
    public class MultipartParseResult
    {
        public NameValueCollection FormData { get; internal set; } = new NameValueCollection();
        public List<Guid> AttachmentIds { get; internal set; } = new List<Guid>();
    }
}