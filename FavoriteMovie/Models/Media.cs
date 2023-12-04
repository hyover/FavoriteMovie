using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FavoriteMovie.Models
{
    public class Media
    {
        // PK
        public int Id { get; set; }

        // Required
        [Required(ErrorMessage = "S'il te plait entre un nom pour le média.")]
        [StringLength(8, ErrorMessage = "Le nom doit avoir une longueur comprise entre {2} and {1}.", MinimumLength = 6)]
        public string Name { get; set; }

        [Required(ErrorMessage = "S'il te plait entre courte note.")]
        [StringLength(255, ErrorMessage = "La note doit avoir une longueur comprise entre {2} and {1}.", MinimumLength = 5)]
        public string Note { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }

        // Optional
        public string? AllocineDescription { get; set; }
        public string? AllocineLink { get; set; }
        public string? StreamingLink { get; set; }
       
        // FK
        public int MediaTypeId { get; set; }
        public int UserId { get; set; }
        public virtual IdentityUser User { get; set; }
        public virtual MediaType MediaType { get; set; }
        public virtual ICollection<MediaGenre> ListMediaGenre { get; set; }

        // Constructor for CreatedAt
        public Media()
        {
            CreatedAt = DateTime.Now;
        }

        // Function call before add or update an entity
        public void BeforeSaveChanges()
        {
            UpdatedAt = DateTime.Now;
        }
    }
}
