using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Rings.Models
{
    public class CreateRingModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}