using Microsoft.AspNetCore.Identity;

namespace FavoriteMovie.Models
{
    public class MediaFavorite
    {
        // PK
        public int Id { get; set; }

        // FK
        public virtual IdentityUser User { get; set; }
        public virtual Media Media { get; set; }
    }
}
