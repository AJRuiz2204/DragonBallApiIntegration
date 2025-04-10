using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DragonBallApi.Models.Database
{
    public class Character
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Ki { get; set; }
        [Required]
        [MaxLength(25)]
        public string Race { get; set; } = string.Empty;
        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string Affiliation { get; set; } = string.Empty;
        public virtual ICollection<Transformation> Transformations { get; set; } = new List<Transformation>();
    }
}