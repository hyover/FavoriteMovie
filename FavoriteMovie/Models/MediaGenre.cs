using System.ComponentModel.DataAnnotations;

namespace FavoriteMovie.Models
{
    public class MediaGenre
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Date)]
        public DateTime UpdatedAt { get; set; }
        public virtual ICollection<Media> Medias { get; set; }

        // Constructor for CreatedAt
        public MediaGenre()
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
