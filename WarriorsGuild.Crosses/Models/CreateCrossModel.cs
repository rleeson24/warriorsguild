using System.ComponentModel.DataAnnotations;

namespace WarriorsGuild.Crosses.Models
{
    public class CreateCrossModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}