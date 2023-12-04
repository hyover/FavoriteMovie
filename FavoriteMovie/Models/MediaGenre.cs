using System.ComponentModel.DataAnnotations;

namespace FavoriteMovie.Models
{
    public class MediaGenre
    {
        // PK
        public int Id { get; set; }

        // Required
        [Required(ErrorMessage = "S'il te plait entre un nom pour le genre.")]
        [StringLength(255, ErrorMessage = "Le nom doit avoir une longueur comprise entre {2} and {1}.", MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }

        // FK
        public virtual ICollection<Media> Medias { get; set; }

        // Constructor for CreatedAt
        public MediaGenre()
        {
            CreatedAt = DateTime.UtcNow;
        }

        // Function call before add or update an entity
        public void BeforeSaveChanges()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
