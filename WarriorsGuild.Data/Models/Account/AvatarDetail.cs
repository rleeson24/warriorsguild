using System;
using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Data.Models.Account
{
    public class AvatarDetail
    {
        [Key]
        public Guid UserId { get; set; }
        public byte[] Data { get; set; }
        public string Extension { get; set; }
        public string ContentType { get; set; }
    }
}