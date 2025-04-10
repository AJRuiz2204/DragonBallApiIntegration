using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DragonBallApi.Models.Database
{
    public class Transformation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Ki { get; set; }
        public int CharacterId { get; set; }
        [ForeignKey("CharacterId")]
        public virtual Character? Character { get; set; }
    }
}