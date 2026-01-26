using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Ranks.Models
{
    public class CreateRankModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}