using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.DataAccess.Models
{
    public class SingleUseFileDownloadKey
    {
        [Key]
        public Guid Key { get; set; }
        public Guid AttachmentId { get; set; }
    }
}